using Notifications.Abstractions;
using SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Notifications.Infrastructure.Channels;

public sealed class EmailNotificationChannel : INotificationChannel
{
    private readonly ILogger<EmailNotificationChannel> _logger;
    private readonly EmailSettings _settings;

    public EmailNotificationChannel(
        ILogger<EmailNotificationChannel> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        var section = configuration.GetSection("EmailNotifications");
        _settings = section.Get<EmailSettings>() ?? new EmailSettings();
    }

    public string ChannelName => "email";

    public async Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_settings.SimulateOnly)
            {
                return await SimulateEmailSendAsync(message);
            }

            return await SendRealEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {Recipient}", message.Recipient);
            return new NotificationResult(
                Channel: ChannelName,
                Status: NotificationStatus.Failed,
                ErrorMessage: ex.Message,
                SentAt: DateTime.UtcNow
            );
        }
    }

    private async Task<NotificationResult> SimulateEmailSendAsync(NotificationMessage message)
    {
        _logger.LogInformation(
            "📧 SIMULATED EMAIL SENT 📧\n" +
            "To: {Recipient}\n" +
            "Subject: {Subject}\n" +
            "Content: {Content}\n" +
            "Metadata: {Metadata}",
            message.Recipient,
            message.Metadata?.GetValueOrDefault("Subject", message.Title) ?? message.Title,
            message.Body,
            message.Metadata != null ? string.Join(", ", message.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}")) : "None"
        );

        // Simulate network delay
        await Task.Delay(100);

        return new NotificationResult(
            Channel: ChannelName,
            Status: NotificationStatus.Delivered,
            ErrorMessage: null,
            SentAt: DateTime.UtcNow
        );
    }

    private async Task<NotificationResult> SendRealEmailAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        var subject = message.Metadata?.GetValueOrDefault("Subject", message.Title)?.ToString() ?? message.Title;
        var isHtml = message.Metadata?.ContainsKey("IsHtml") == true &&
                     message.Metadata["IsHtml"] is bool htmlFlag && htmlFlag;

        using var client = new SmtpClient(_settings.Host, _settings.Port);

        if (_settings.UseAuthentication)
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        client.EnableSsl = _settings.UseSsl;

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = message.Body,
            IsBodyHtml = isHtml
        };

        mailMessage.To.Add(message.Recipient);

        // Add additional headers from metadata
        if (message.Metadata != null)
        {
            foreach (var kvp in message.Metadata.Where(m => m.Key.StartsWith("Header-")))
            {
                var headerName = kvp.Key[7..]; // Remove "Header-" prefix
                mailMessage.Headers.Add(headerName, kvp.Value?.ToString() ?? string.Empty);
            }
        }

        await client.SendMailAsync(mailMessage, cancellationToken);

        _logger.LogInformation("Email sent successfully to {Recipient}", message.Recipient);

        return new NotificationResult(
            Channel: ChannelName,
            Status: NotificationStatus.Delivered,
            ErrorMessage: null,
            SentAt: DateTime.UtcNow
        );
    }

    public Task<bool> ValidateConfigurationAsync()
    {
        var isValid = !string.IsNullOrEmpty(_settings.Host) && _settings.Port > 0;

        if (!isValid)
        {
            _logger.LogWarning("Email notification channel configuration is invalid");
        }

        return Task.FromResult(isValid);
    }
}

public sealed class EmailSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public bool UseAuthentication { get; set; } = false;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@cmc-app.com";
    public string FromName { get; set; } = "CMC Todo App";
    public bool SimulateOnly { get; set; } = true;
}
