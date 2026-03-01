using SharedKernel;

namespace Notifications.Abstractions;

/// <summary>
/// Schweregrad einer Notification – steuert visuelle Darstellung (Farbe, Icon)
/// unabhängig vom Delivery-Channel.
/// </summary>
public enum NotificationSeverity
{
    Info    = 0,
    Success = 1,
    Warning = 2,
    Error   = 3,
    Notice  = 4
}

/// <summary>
/// Art einer Nutzer-Interaktion, die einer Notification angehängt werden kann.
/// </summary>
public enum NotificationInteractionType
{
    /// <summary>Navigations-Link (client-seitig, href).</summary>
    Link         = 0,
    /// <summary>Server-seitige Aktion (wird per ActionName ausgelöst).</summary>
    ServerAction = 1
}

/// <summary>
/// Eine einzelne Interaktionsmöglichkeit (z.B. „Passwort ändern" oder „Tour-Bestätigung").
/// Value Object – unveränderlich, identifiziert über alle Felder.
/// </summary>
public sealed record NotificationInteraction : ValueObject
{
    public NotificationInteraction() { } // EF Core / System.Text.Json benötigt zugänglichen Konstruktor

    /// <summary>Eindeutiger Identifier der Interaktion (für Client-seitiges Tracking).</summary>
    public Guid   InteractionId { get; init; } = Guid.NewGuid();
    public string Label         { get; init; } = string.Empty;
    public NotificationInteractionType Type { get; init; }

    /// <summary>Ziel-URL – nur gültig wenn <see cref="Type"/> == Link.</summary>
    public string? Href       { get; init; }

    /// <summary>Eindeutiger Aktionsname – nur gültig wenn <see cref="Type"/> == ServerAction.</summary>
    public string? ActionName { get; init; }

    // ── Factory-Methoden ──────────────────────────────────────────────────────

    public static NotificationInteraction CreateLink(string label, string href)
        => new() { Label = label, Type = NotificationInteractionType.Link, Href = href };

    public static NotificationInteraction CreateServerAction(string label, string actionName)
        => new() { Label = label, Type = NotificationInteractionType.ServerAction, ActionName = actionName };
}

/// <summary>Stark typisierter Identifier für persistente Notifications.</summary>
public sealed record NotificationId : ValueObject
{
    private NotificationId(Guid value) => Value = value;

    public Guid Value { get; }

    public static NotificationId New()            => new(Guid.NewGuid());
    public static NotificationId From(Guid value) => new(value);

    public static implicit operator Guid(NotificationId id) => id.Value;
}
