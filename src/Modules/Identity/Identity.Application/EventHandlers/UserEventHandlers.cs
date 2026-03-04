using Identity.Application.Mailing;
using Identity.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediatR;
using Notifications.Abstractions;

namespace Identity.Application.EventHandlers;

public sealed class UserPasswordResetRequestedHandler : INotificationHandler<UserPasswordResetRequestedEvent>
{
    private readonly Notifications.Abstractions.INotificationPublisher _notificationPublisher;
    private readonly ILogger<UserPasswordResetRequestedHandler> _logger;
    private readonly string _baseUrl;

    public UserPasswordResetRequestedHandler(
        Notifications.Abstractions.INotificationPublisher notificationPublisher,
        ILogger<UserPasswordResetRequestedHandler> logger,
        IConfiguration configuration)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
        _baseUrl = configuration["ApiSettings:FrontendBaseUrl"]?.TrimEnd('/')
                ?? configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? "https://app.yourdomain.com";
    }

    public async Task Handle(UserPasswordResetRequestedEvent notification, CancellationToken cancellationToken)
    {
        var resetUrl = $"{_baseUrl}/reset-password"
                     + $"?email={Uri.EscapeDataString(notification.Email)}"
                     + $"&token={Uri.EscapeDataString(notification.ResetToken)}";

        var message = new NotificationMessage(
            NotificationChannels.Email,
            notification.Email,
            "Passwort zurücksetzen",
            EmailTemplates.PasswordReset(resetUrl),
            metadata: new Dictionary<string, object>
            {
                ["IsHtml"]     = true,
                ["UserId"]     = notification.UserId.Value,
                ["ResetToken"] = notification.ResetToken,
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
            _logger.LogError("Failed to send password reset email to {Email}: {Error}", notification.Email, result.Error);
        else
            _logger.LogInformation("Password reset email sent to {Email}", notification.Email);
    }
}

public sealed class UserRegisteredHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly Notifications.Abstractions.INotificationPublisher _notificationPublisher;
    private readonly ILogger<UserRegisteredHandler> _logger;

    public UserRegisteredHandler(
        Notifications.Abstractions.INotificationPublisher notificationPublisher,
        ILogger<UserRegisteredHandler> logger)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var message = new NotificationMessage(
            NotificationChannels.Email,
            notification.Email,
            "Willkommen bei CMC!",
            EmailTemplates.Welcome(string.Empty),
            metadata: new Dictionary<string, object>
            {
                ["IsHtml"]  = true,
                ["UserId"]  = notification.UserId.Value,
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
            _logger.LogError("Failed to send welcome email to {Email}: {Error}", notification.Email, result.Error);
        else
            _logger.LogInformation("Welcome email sent to {Email}", notification.Email);
    }
}

/// <summary>
/// Versendet die E-Mail-Bestätigungs-E-Mail mit dem Verifizierungstoken.
/// </summary>
public sealed class UserEmailVerificationRequestedHandler : INotificationHandler<UserEmailVerificationRequestedEvent>
{
    private readonly Notifications.Abstractions.INotificationPublisher _notificationPublisher;
    private readonly ILogger<UserEmailVerificationRequestedHandler> _logger;
    private readonly string _baseUrl;

    public UserEmailVerificationRequestedHandler(
        Notifications.Abstractions.INotificationPublisher notificationPublisher,
        ILogger<UserEmailVerificationRequestedHandler> logger,
        IConfiguration configuration)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
        _baseUrl = configuration["ApiSettings:FrontendBaseUrl"]?.TrimEnd('/')
                ?? configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? "https://app.yourdomain.com";
    }

    public async Task Handle(UserEmailVerificationRequestedEvent notification, CancellationToken cancellationToken)
    {
        var verifyUrl = $"{_baseUrl}/verify-mail"
                      + $"?email={Uri.EscapeDataString(notification.Email)}"
                      + $"&token={Uri.EscapeDataString(notification.VerificationToken)}";

        var message = new NotificationMessage(
            NotificationChannels.Email,
            notification.Email,
            "E-Mail-Adresse bestätigen",
            EmailTemplates.EmailVerification(notification.VerificationToken, verifyUrl),
            metadata: new Dictionary<string, object>
            {
                ["IsHtml"]            = true,
                ["UserId"]            = notification.UserId.Value,
                ["VerificationToken"] = notification.VerificationToken,
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
            _logger.LogError("Failed to send verification email to {Email}: {Error}", notification.Email, result.Error);
        else
            _logger.LogInformation("Email verification mail sent to {Email}", notification.Email);
    }
}
