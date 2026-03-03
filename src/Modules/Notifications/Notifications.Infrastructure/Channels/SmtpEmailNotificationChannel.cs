using Notifications.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Notifications.Infrastructure.Channels;

/// <summary>
/// SMTP-E-Mail-Channel. Unterstützt:
/// - SimulateOnly = true  → nur Logging (Dev-Standard)
/// - MailHog              → Host=mailhog, Port=1025, kein TLS, kein Auth
/// - Echter SMTP          → beliebiger Host + optionale Auth + TLS
///
/// Konfiguration via appsettings:
///   "Email": { "SmtpHost": "mailhog", "SmtpPort": 1025, "SimulateOnly": false, ... }
/// </summary>
public sealed class SmtpEmailNotificationChannel : IEmailNotificationChannel
{
    private readonly SmtpEmailSettings                  _settings;
    private readonly ILogger<SmtpEmailNotificationChannel> _logger;

    public string ChannelName => NotificationChannels.Email;

    public SmtpEmailNotificationChannel(
        IOptions<SmtpEmailSettings>                options,
        ILogger<SmtpEmailNotificationChannel>      logger)
    {
        _settings = options.Value;
        _logger   = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationMessage message,
        CancellationToken   cancellationToken = default)
    {
        if (_settings.SimulateOnly)
            return Simulate(message);

        try
        {
            using var client = BuildSmtpClient();
            using var mail   = BuildMailMessage(message);
            await client.SendMailAsync(mail, cancellationToken);

            _logger.LogInformation("E-Mail gesendet an {Recipient} via {Host}:{Port}",
                message.Recipient, _settings.SmtpHost, _settings.SmtpPort);

            return new NotificationResult(ChannelName, NotificationStatus.Sent, null, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim E-Mail-Versand an {Recipient}", message.Recipient);
            return new NotificationResult(ChannelName, NotificationStatus.Failed, ex.Message, DateTime.UtcNow);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private NotificationResult Simulate(NotificationMessage message)
    {
        _logger.LogInformation(
            "[SIMULATED] Email would be sent to {Recipient} with title '{Title}'",
            message.Recipient, message.Title);
        return new NotificationResult(ChannelName, NotificationStatus.Sent, null, DateTime.UtcNow);
    }

    private SmtpClient BuildSmtpClient()
    {
        var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl       = _settings.UseSsl,
            DeliveryMethod  = SmtpDeliveryMethod.Network,
            Timeout         = _settings.TimeoutMs,
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        return client;
    }

    private MailMessage BuildMailMessage(NotificationMessage message)
    {
        var mail = new MailMessage
        {
            From       = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject    = message.Title,
            Body       = message.Body,
            IsBodyHtml = message.Metadata?.TryGetValue("IsHtml", out var isHtml) == true && isHtml is true,
        };
        mail.To.Add(message.Recipient);
        return mail;
    }
}

public sealed class SmtpEmailSettings
{
    // Verbindung
    public string SmtpHost    { get; set; } = "localhost";
    public int    SmtpPort    { get; set; } = 1025;
    public bool   UseSsl      { get; set; } = false;
    public int    TimeoutMs   { get; set; } = 5000;

    // Auth (leer = kein Auth, z.B. MailHog)
    public string Username    { get; set; } = string.Empty;
    public string Password    { get; set; } = string.Empty;

    // Absender
    public string FromAddress { get; set; } = "noreply@cmc-app.com";
    public string FromName    { get; set; } = "CMC";

    // Dev-Flag
    public bool   SimulateOnly { get; set; } = false;
}
