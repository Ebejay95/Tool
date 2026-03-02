using MediatR;
using Measures.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Measures.Infrastructure.Outbox;

/// <summary>
/// BackgroundService der unverarbeitete OutboxMessages aus dem Measures-Schema liest
/// und via MediatR dispatcht (Outbox Pattern, at-least-once).
/// FOR UPDATE SKIP LOCKED verhindert Doppelverarbeitung bei horizontaler Skalierung.
/// </summary>
internal sealed class MeasuresOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<MeasuresOutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MeasuresOutboxProcessor gestartet.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Unerwarteter Fehler im MeasuresOutboxProcessor."); }

            await Task.Delay(PollingInterval, stoppingToken).ConfigureAwait(false);
        }

        logger.LogInformation("MeasuresOutboxProcessor gestoppt.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MeasuresDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await context.OutboxMessages
            .FromSqlRaw("""
                SELECT * FROM measures."OutboxMessages"
                WHERE "ProcessedOn" IS NULL
                  AND "RetryCount" < {1}
                ORDER BY "OccurredOn"
                LIMIT {0}
                FOR UPDATE SKIP LOCKED
                """, BatchSize, OutboxMessage.MaxRetries)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) { await transaction.RollbackAsync(cancellationToken); return; }

        foreach (var message in messages)
        {
            try
            {
                await mediator.DispatchOutboxMessageAsync(message, cancellationToken);
                message.ProcessedOn = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fehler beim Verarbeiten von OutboxMessage {MessageId} (Type: {EventType})", message.Id, message.EventType);
                message.Error = ex.Message;
                message.RetryCount++;
                if (message.RetryCount >= OutboxMessage.MaxRetries)
                    logger.LogWarning("OutboxMessage {MessageId} hat MaxRetries ({MaxRetries}) erreicht.", message.Id, OutboxMessage.MaxRetries);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
