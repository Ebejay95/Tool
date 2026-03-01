using Api.Bootstrap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.Abstractions;
using SharedKernel;

namespace Api.Controllers;

/// <summary>
/// Nur im Development-Environment aktiv (via <see cref="DevOnlyConvention"/>).
/// In Production werden die Routen zur Registrierungszeit entfernt –
/// der Endpunkt existiert physisch nicht.
/// </summary>
[DevOnly]
[ApiController]
[Route("api/v1/dev")]
[Authorize]
public sealed class DevController : ControllerBase
{
    private readonly INotificationPublisher _publisher;
    private readonly ICurrentUser           _currentUser;

    public DevController(
        INotificationPublisher publisher,
        ICurrentUser           currentUser)
    {
        _publisher   = publisher;
        _currentUser = currentUser;
    }

    public sealed record SendNotificationRequest(
        string        Title,
        string        Body,
        string        Severity,
        bool          ToCurrentUser,
        List<string>? AdditionalRecipients,
        string[]      Channels);

    [HttpPost("send-notification")]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationRequest req,
        CancellationToken cancellationToken)
    {
        var severity = req.Severity.ToLowerInvariant() switch
        {
            "success" => NotificationSeverity.Success,
            "warning" => NotificationSeverity.Warning,
            "error"   => NotificationSeverity.Error,
            "notice"  => NotificationSeverity.Notice,
            _         => NotificationSeverity.Info
        };

        var recipients = new List<string>();

        if (req.ToCurrentUser)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();
            recipients.Add(_currentUser.UserId.Value.ToString());
        }

        if (req.AdditionalRecipients is { Count: > 0 })
            recipients.AddRange(req.AdditionalRecipients);

        if (recipients.Count == 0)
            return BadRequest("Kein Empfänger angegeben.");

        var messages = recipients.Select(r => new NotificationMessage(
            channels:  req.Channels,
            recipient: r,
            title:     req.Title,
            body:      req.Body,
            severity:  severity));

        var result = await _publisher.PublishBatchAsync(messages, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}
