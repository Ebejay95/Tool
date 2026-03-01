using CMC.Notifications.Abstractions;
using CMC.Todos.Domain.TodoItems;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CMC.Todos.Application.EventHandlers;

public sealed class TodoCompletedHandler : INotificationHandler<TodoCompletedEvent>
{
    private readonly CMC.Notifications.Abstractions.INotificationPublisher _notificationPublisher;
    private readonly ILogger<TodoCompletedHandler> _logger;

    public TodoCompletedHandler(
        CMC.Notifications.Abstractions.INotificationPublisher notificationPublisher,
        ILogger<TodoCompletedHandler> logger)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task Handle(TodoCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Send real-time notification via SignalR (Snackbar im Browser)
        var message = new NotificationMessage(
            NotificationChannels.SignalR,
            notification.UserId.Value.ToString(),
            "Todo Completed",
            $"Congratulations! You completed '{notification.Title}'",
            metadata: new Dictionary<string, object>
            {
                ["TodoId"] = notification.TodoId.Value,
                ["Type"] = "TodoCompleted"
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to send todo completion notification to user {UserId}: {Error}",
                notification.UserId, result.Error);
        }
        else
        {
            _logger.LogInformation("Todo completion notification sent to user {UserId} for todo '{Title}'",
                notification.UserId, notification.Title);
        }
    }
}

public sealed class TodoCreatedHandler : INotificationHandler<TodoCreatedEvent>
{
    private readonly CMC.Notifications.Abstractions.INotificationPublisher _notificationPublisher;
    private readonly ILogger<TodoCreatedHandler> _logger;

    public TodoCreatedHandler(
        CMC.Notifications.Abstractions.INotificationPublisher notificationPublisher,
        ILogger<TodoCreatedHandler> logger)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task Handle(TodoCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Send real-time notification via SignalR (Snackbar im Browser)
        var message = new NotificationMessage(
            NotificationChannels.SignalR,
            notification.UserId.Value.ToString(),
            "New Todo Created",
            $"Todo '{notification.Title}' has been created",
            metadata: new Dictionary<string, object>
            {
                ["TodoId"] = notification.TodoId.Value,
                ["Type"] = "TodoCreated"
            });

        var result = await _notificationPublisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to send todo creation notification to user {UserId}: {Error}",
                notification.UserId, result.Error);
        }
        else
        {
            _logger.LogDebug("Todo creation notification sent to user {UserId} for todo '{Title}'",
                notification.UserId, notification.Title);
        }
    }
}
