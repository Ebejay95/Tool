using MediatR;
using System.Text.Json;

namespace SharedKernel;

/// <summary>
/// Kapselt das standardisierte Domain-Event-Handling.
///
/// WICHTIG – Outbox Pattern:
///   Domain-Events werden NICHT mehr direkt in SaveChangesAsync dispatcht.
///   Stattdessen werden sie als OutboxMessage-Zeilen in derselben Transaktion gespeichert.
///   Ein BackgroundService (OutboxProcessor) dispatcht sie anschließend via MediatR.
///
///   Ablauf:
///     1. SaveChangesAsync() → CollectOutboxMessages() → OutboxMessages in DB schreiben (atomar)
///     2. OutboxProcessor (BackgroundService) → liest unverarbeitete Nachrichten
///     3. DispatchDomainEventsAsync() (aus OutboxProcessor) → dispatcht via MediatR
///     4. ProcessedOn wird gesetzt
/// </summary>
public static class MediatorExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    /// <summary>
    /// Konvertiert alle Domain-Events der übergebenen Entitäten in OutboxMessages
    /// und leert danach die Event-Listen der Entitäten.
    /// Wird aus SaveChangesAsync() vor base.SaveChangesAsync() aufgerufen.
    /// </summary>
    public static IReadOnlyList<OutboxMessage> CollectOutboxMessages(IEnumerable<Entity> entities)
    {
        var entityList = entities.ToList();

        var messages = entityList
            .SelectMany(e => e.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                EventType  = domainEvent.GetType().AssemblyQualifiedName!,
                Payload    = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions),
                OccurredOn = domainEvent.OccurredOn
            })
            .ToList();

        foreach (var entity in entityList)
            entity.ClearDomainEvents();

        return messages;
    }

    /// <summary>
    /// Dispatcht eine deserialisierte Domain-Event-Nachricht via MediatR.
    /// Wird ausschließlich vom OutboxProcessor aufgerufen.
    /// </summary>
    public static async Task DispatchOutboxMessageAsync(
        this IMediator mediator,
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        var eventType = Type.GetType(message.EventType);
        if (eventType is null)
            throw new InvalidOperationException($"Unknown domain event type: {message.EventType}");

        var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
        if (domainEvent is not null)
            await mediator.Publish(domainEvent, cancellationToken);
    }
}
