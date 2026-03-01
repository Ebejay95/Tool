using Identity.Domain.Users;
using Notifications.Abstractions;
using SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.EventHandlers;

public sealed class UserPasswordResetRequestedHandler : INotificationHandler<UserPasswordResetRequestedEvent>
{
    private readonly Notifications.Abstractions.INotificationPublisher _notificationPublisher;
    private readonly ILogger<UserPasswordResetRequestedHandler> _logger;

    public UserPasswordResetRequestedHandler(
        Notifications.Abstractions.INotificationPublisher notificationPublisher,
        ILogger<UserPasswordResetRequestedHandler> logger)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task Handle(UserPasswordResetRequestedEvent notification, CancellationToken cancellationToken)
    {
        var message = new NotificationMessage(
            NotificationChannels.Email,
            notification.Email,
            "Password Reset Request",
            $"""
            Hello,

            You have requested a password reset for your account.

            Your reset code is: {notification.ResetToken}

            This code will expire in 1 hour.

            If you did not request this reset, please ignore this email.

            Best regards,
            CMC Team
            """,
            metadata: new Dictionary<string, object>
            {
                ["UserId"] = notification.UserId.Value,
                ["ResetToken"] = notification.ResetToken
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to send password reset email to {Email}: {Error}",
                notification.Email, result.Error);
        }
        else
        {
            _logger.LogInformation("Password reset email sent to {Email}", notification.Email);
        }
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
            "Welcome to CMC!",
            $"""
            Hello,

            Welcome to CMC! Your account has been successfully created.

            You can now log in and start using our services.

            Best regards,
            CMC Team
            """,
            metadata: new Dictionary<string, object>
            {
                ["UserId"] = notification.UserId.Value
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to send welcome email to {Email}: {Error}",
                notification.Email, result.Error);
        }
        else
        {
            _logger.LogInformation("Welcome email sent to {Email}", notification.Email);
        }
    }
}
