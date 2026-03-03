namespace Notifications.Application.DTOs;

public sealed record NotificationInteractionDto(
    Guid    InteractionId,
    string  Label,
    string  Type,
    string? Href,
    string? ActionName);

public sealed record NotificationDto(
    Guid                                      Id,
    string                                    Title,
    string                                    Body,
    string                                    Severity,
    bool                                      IsRead,
    DateTime?                                 ExpiresAt,
    DateTime                                  CreatedAt,
    IReadOnlyList<NotificationInteractionDto> Interactions);
