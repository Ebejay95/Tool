using MediatR;
using Microsoft.Extensions.Logging;
using Notifications.Abstractions;
using Todos.Domain.Todos;
using INotificationPublisher = Notifications.Abstractions.INotificationPublisher;

namespace Todos.Application.EventHandlers;

/// <summary>
/// Reagiert auf Todo-Domain-Events und sendet entsprechende Echtzeit-Benachrichtigungen.
///
/// Der Handler lebt bewusst in Todos.Application, nicht in Notifications.Application:
/// Todos kennt seine eigenen Domain-Events (TodoCreatedEvent, TodoCompletedEvent) und
/// Notifications.Abstractions ist WASM-safe und kann referenziert werden.
/// Damit besteht keinerlei direkte Abhängigkeit mehr von Notifications auf Todos.
/// </summary>
public sealed class TodoCreatedNotificationHandler : INotificationHandler<TodoCreatedEvent>
{
    private readonly INotificationPublisher _publisher;
    private readonly ILogger<TodoCreatedNotificationHandler> _logger;

    public TodoCreatedNotificationHandler(
        INotificationPublisher publisher,
        ILogger<TodoCreatedNotificationHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(TodoCreatedEvent notification, CancellationToken cancellationToken)
    {
        var message = new NotificationMessage(
            NotificationChannels.SignalR,
            notification.UserId.Value.ToString(),
            "New Todo Created",
            $"Todo '{notification.Title}' has been created",
            metadata: new Dictionary<string, object>
            {
                ["TodoId"] = notification.TodoId.Value,
                ["Type"]   = "TodoCreated"
            });

        var result = await _publisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
            _logger.LogError("Failed to send TodoCreated notification for user {UserId}: {Error}",
                notification.UserId, result.Error);
        else
            _logger.LogDebug("TodoCreated notification sent to user {UserId} for todo '{Title}'",
                notification.UserId, notification.Title);
    }
}

public sealed class TodoCompletedNotificationHandler : INotificationHandler<TodoCompletedEvent>
{
    private readonly INotificationPublisher _publisher;
    private readonly ILogger<TodoCompletedNotificationHandler> _logger;

    public TodoCompletedNotificationHandler(
        INotificationPublisher publisher,
        ILogger<TodoCompletedNotificationHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(TodoCompletedEvent notification, CancellationToken cancellationToken)
    {
        var message = new NotificationMessage(
            NotificationChannels.SignalR,
            notification.UserId.Value.ToString(),
            "Todo Completed",
            $"Congratulations! You completed '{notification.Title}'",
            metadata: new Dictionary<string, object>
            {
                ["TodoId"] = notification.TodoId.Value,
                ["Type"]   = "TodoCompleted"
            });

        var result = await _publisher.PublishAsync(message, cancellationToken);

        if (result.IsFailure)
            _logger.LogError("Failed to send TodoCompleted notification for user {UserId}: {Error}",
                notification.UserId, result.Error);
        else
            _logger.LogInformation("TodoCompleted notification sent to user {UserId} for todo '{Title}'",
                notification.UserId, notification.Title);
    }
}
