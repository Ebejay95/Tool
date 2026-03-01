namespace SharedKernel;

/// <summary>
/// Persistierte Darstellung eines Domain-Events in der Outbox-Tabelle.
///
/// Funktionsweise (Transaktionale Outbox):
///   1. SaveChangesAsync() schreibt Domänendaten UND OutboxMessages in EINER Transaktion.
///   2. Ein BackgroundService (OutboxProcessor) liest unverarbeitete Nachrichten und dispatcht sie via MediatR.
///   3. Nach erfolgreichem Dispatch wird ProcessedOn gesetzt.
///
/// Vorteil: Kein Datenverlust, da Event-Serialisierung und Datenpersistierung atomar sind.
/// Selbst ein Absturz nach dem Commit lässt die Nachricht in der DB zurück → Retry beim nächsten Start.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Assembly-qualifizierter Typname des Domain-Events für polymorphe Deserialisierung.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>JSON-serialisiertes Domain-Event Payload.</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>Zeitpunkt an dem das Event erzeugt wurde.</summary>
    public DateTimeOffset OccurredOn { get; init; }

    /// <summary>Null = noch nicht verarbeitet. Gesetzt nach erfolgreichem MediatR-Dispatch.</summary>
    public DateTimeOffset? ProcessedOn { get; set; }

    /// <summary>Letzter Fehler beim Dispatch-Versuch. Null = kein Fehler aufgetreten.</summary>
    public string? Error { get; set; }

    /// <summary>Anzahl bisheriger Dispatch-Versuche. Wird bei jedem Fehlschlag hochgezählt.</summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Nachrichten die MaxRetries erreicht haben gelten als Dead Letter und werden
    /// vom OutboxProcessor nicht mehr versucht. Manueller Eingriff erforderlich.
    /// </summary>
    public const int MaxRetries = 5;
}
