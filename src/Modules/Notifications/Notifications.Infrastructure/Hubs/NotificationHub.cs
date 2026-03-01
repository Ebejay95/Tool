using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Notifications.Infrastructure.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    // Server → Client: "ReceiveNotification"
    // Clients verbinden sich mit JWT-Token (Bearer) über den Access-Token-Provider
}
