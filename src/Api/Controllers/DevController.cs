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

        if (req.ToCurrentUser && _currentUser.UserId is null)
            return Unauthorized();

        var messages = new List<NotificationMessage>();

        // ── Nicht-Email-Channels: Recipient = User-ID ─────────────────────
        var nonEmailChannels = req.Channels
            .Where(c => !c.Equals("email", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (nonEmailChannels.Length > 0)
        {
            var recipients = new List<string>();

            if (req.ToCurrentUser)
                recipients.Add(_currentUser.UserId!.Value.ToString());

            if (req.AdditionalRecipients is { Count: > 0 })
                recipients.AddRange(req.AdditionalRecipients);

            if (recipients.Count == 0)
                return BadRequest("Kein Empfänger angegeben.");

            messages.AddRange(recipients.Select(r => new NotificationMessage(
                channels:  nonEmailChannels,
                recipient: r,
                title:     req.Title,
                body:      req.Body,
                severity:  severity)));
        }

        // ── Email-Channel: Recipient = E-Mail-Adresse ─────────────────────
        if (req.Channels.Any(c => c.Equals("email", StringComparison.OrdinalIgnoreCase)))
        {
            if (req.ToCurrentUser && !string.IsNullOrEmpty(_currentUser.Email))
            {
                messages.Add(new NotificationMessage(
                    channels:  ["email"],
                    recipient: _currentUser.Email,
                    title:     req.Title,
                    body:      req.Body,
                    severity:  severity));
            }
        }

        if (messages.Count == 0)
            return BadRequest("Kein Empfänger angegeben.");

        var result = await _publisher.PublishBatchAsync(messages, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}
