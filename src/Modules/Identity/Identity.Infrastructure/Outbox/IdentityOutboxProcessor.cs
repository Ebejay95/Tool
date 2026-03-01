using Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Identity.Infrastructure.Outbox;

/// <summary>
/// BackgroundService der unverarbeitete OutboxMessages aus der Identity-Datenbank liest
/// und via MediatR dispatcht.
/// Gleiche Garantien und SKIP LOCKED-Mechanismus wie TodosOutboxProcessor – siehe dort für Details.
/// </summary>
internal sealed class IdentityOutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdentityOutboxProcessor> _logger;

    public IdentityOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<IdentityOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IdentityOutboxProcessor gestartet.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unerwarteter Fehler im IdentityOutboxProcessor.");
            }

            await Task.Delay(PollingInterval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("IdentityOutboxProcessor gestoppt.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context           = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var mediator          = scope.ServiceProvider.GetRequiredService<IMediator>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await context.OutboxMessages
            .FromSqlRaw("""
                SELECT * FROM identity."OutboxMessages"
                WHERE "ProcessedOn" IS NULL
                  AND "RetryCount" < {1}
                ORDER BY "OccurredOn"
                LIMIT {0}
                FOR UPDATE SKIP LOCKED
                """, BatchSize, OutboxMessage.MaxRetries)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                await mediator.DispatchOutboxMessageAsync(message, cancellationToken);
                message.ProcessedOn = DateTimeOffset.UtcNow;
                message.Error       = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Verarbeiten von OutboxMessage {MessageId} (Type: {EventType})",
                    message.Id, message.EventType);
                message.Error = ex.Message;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
