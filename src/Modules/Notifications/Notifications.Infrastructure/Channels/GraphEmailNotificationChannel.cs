using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Notifications.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Notifications.Infrastructure.Channels;

/// <summary>
/// Microsoft Graph API E-Mail-Channel für die Produktionsumgebung.
///
/// Voraussetzungen (Azure AD App-Registrierung):
///   - API-Berechtigungen: Mail.Send (Application, nicht Delegated)
///   - Client Secret oder Zertifikat
///
/// Konfiguration via K8s Secret → appsettings:
///   "Email":
///     "Graph":
///       TenantId:    "..."
///       ClientId:    "..."
///       ClientSecret:"..."
///       FromAddress: "noreply@yourdomain.com"
///       FromName:    "CMC"
/// </summary>
public sealed class GraphEmailNotificationChannel : IEmailNotificationChannel
{
    private readonly GraphEmailSettings                  _settings;
    private readonly ILogger<GraphEmailNotificationChannel> _logger;
    private readonly Lazy<GraphServiceClient>            _graphClient;

    public string ChannelName => NotificationChannels.Email;

    public GraphEmailNotificationChannel(
        IOptions<GraphEmailSettings>                options,
        ILogger<GraphEmailNotificationChannel>      logger)
    {
        _settings    = options.Value;
        _logger      = logger;
        _graphClient = new Lazy<GraphServiceClient>(BuildClient);
    }

    public async Task<NotificationResult> SendAsync(
        NotificationMessage message,
        CancellationToken   cancellationToken = default)
    {
        try
        {
            var requestBody = new SendMailPostRequestBody
            {
                Message = new Message
                {
                    Subject = message.Title,
                    Body = new ItemBody
                    {
                        ContentType = message.Metadata?.TryGetValue("IsHtml", out var isHtml) == true
                                          && isHtml is true
                                      ? BodyType.Html
                                      : BodyType.Text,
                        Content     = message.Body,
                    },
                    ToRecipients = new List<Recipient>
                    {
                        new() { EmailAddress = new EmailAddress { Address = message.Recipient } }
                    },
                    From = new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address  = _settings.FromAddress,
                            Name     = _settings.FromName,
                        }
                    }
                },
                SaveToSentItems = false,
            };

            // Sendet im Namen der From-Adresse (Shared Mailbox / Service Account)
            await _graphClient.Value
                .Users[_settings.FromAddress]
                .SendMail
                .PostAsync(requestBody, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Graph-E-Mail gesendet an {Recipient} von {From}",
                message.Recipient, _settings.FromAddress);

            return new NotificationResult(ChannelName, NotificationStatus.Sent, null, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fehler beim Graph-E-Mail-Versand an {Recipient}", message.Recipient);
            return new NotificationResult(ChannelName, NotificationStatus.Failed, ex.Message, DateTime.UtcNow);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private GraphServiceClient BuildClient()
    {
        var credential = new ClientSecretCredential(
            tenantId:     _settings.TenantId,
            clientId:     _settings.ClientId,
            clientSecret: _settings.ClientSecret);

        return new GraphServiceClient(credential,
            ["https://graph.microsoft.com/.default"]);
    }
}

public sealed class GraphEmailSettings
{
    public string TenantId      { get; set; } = string.Empty;
    public string ClientId      { get; set; } = string.Empty;
    public string ClientSecret  { get; set; } = string.Empty;
    public string FromAddress   { get; set; } = string.Empty;
    public string FromName      { get; set; } = "CMC";
}
