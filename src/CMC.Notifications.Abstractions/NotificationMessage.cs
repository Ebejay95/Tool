using System.Collections.Immutable;

namespace CMC.Notifications.Abstractions;

public sealed record NotificationMessage(
    string Channel,
    string To,
    string Subject,
    string Body,
    ImmutableDictionary<string, string>? Metadata = null);
