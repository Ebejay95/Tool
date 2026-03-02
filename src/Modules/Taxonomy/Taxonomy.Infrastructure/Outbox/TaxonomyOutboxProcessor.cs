using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Taxonomy.Infrastructure.Persistence;

namespace Taxonomy.Infrastructure.Outbox;

/// <summary>
/// BackgroundService der unverarbeitete OutboxMessages aus dem Taxonomy-Schema liest
/// und via MediatR dispatcht.
/// </summary>
internal sealed class TaxonomyOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<TaxonomyOutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TaxonomyOutboxProcessor gestartet.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Unerwarteter Fehler im TaxonomyOutboxProcessor."); }

            await Task.Delay(PollingInterval, stoppingToken).ConfigureAwait(false);
        }

        logger.LogInformation("TaxonomyOutboxProcessor gestoppt.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope    = scopeFactory.CreateAsyncScope();
        var context  = scope.ServiceProvider.GetRequiredService<TaxonomyDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await context.OutboxMessages
            .FromSqlRaw("""
                SELECT * FROM taxonomy."OutboxMessages"
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
