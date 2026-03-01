using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Todos.Infrastructure.Persistence;

namespace Todos.Infrastructure.Outbox;

/// <summary>
/// BackgroundService der unverarbeitete OutboxMessages aus der Todos-Datenbank liest
/// und via MediatR dispatcht.
///
/// Outbox-Garantie:
///   - Pollt alle 5 Sekunden nach unverarbeiteten Nachrichten (ProcessedOn IS NULL).
///   - Jede Nachricht wird deserialisiert und über MediatR.Publish() weitergeleitet.
///   - Nach erfolgreichem Dispatch wird ProcessedOn gesetzt (= "acknowledgement").
///   - Fehler werden geloggt und gespeichert; die Nachricht bleibt unverarbeitet für Retry.
///
/// Horizontal Scaling:
///   PostgreSQL FOR UPDATE SKIP LOCKED verhindert Doppelverarbeitung wenn mehrere
///   Api-Instanzen gleichzeitig denselben Batch lesen wollen. Jede Instanz überspringt
///   bereits gesperrte Zeilen und bearbeitet nur die eigene, nicht gesperrte Teilmenge.
/// </summary>
internal sealed class TodosOutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TodosOutboxProcessor> _logger;

    public TodosOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<TodosOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TodosOutboxProcessor gestartet.");

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
                _logger.LogError(ex, "Unerwarteter Fehler im TodosOutboxProcessor.");
            }

            await Task.Delay(PollingInterval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("TodosOutboxProcessor gestoppt.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope   = _scopeFactory.CreateAsyncScope();
        var context             = scope.ServiceProvider.GetRequiredService<TodosDbContext>();
        var mediator            = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Transaktion für FOR UPDATE SKIP LOCKED:
        // Der Lock bleibt bestehen bis zum Commit → andere Instanzen überspringen diese Zeilen.
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await context.OutboxMessages
            .FromSqlRaw("""
                SELECT * FROM "OutboxMessages"
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
                message.RetryCount++;
                if (message.RetryCount >= OutboxMessage.MaxRetries)
                    _logger.LogWarning(
                        "OutboxMessage {MessageId} hat MaxRetries ({MaxRetries}) erreicht und wird nicht mehr automatisch versucht.",
                        message.Id, OutboxMessage.MaxRetries);
                // ProcessedOn bleibt null → Retry beim nächsten Poll (bis MaxRetries)
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
