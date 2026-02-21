using CMC.Notifications.Abstractions;
using CMC.Persistence;
using Microsoft.AspNetCore.Identity;

namespace CMC.Web.Components.Account;

internal sealed class NotificationEmailSender(INotificationPublisher publisher) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => publisher.PublishAsync(new NotificationMessage(
            Channel: "email",
            To: email,
            Subject: "Confirm your email",
            Body: $"Please confirm your account: {confirmationLink}"));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => publisher.PublishAsync(new NotificationMessage(
            Channel: "email",
            To: email,
            Subject: "Reset your password",
            Body: $"Reset your password: {resetLink}"));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => publisher.PublishAsync(new NotificationMessage(
            Channel: "email",
            To: email,
            Subject: "Reset your password",
            Body: $"Reset code: {resetCode}"));
}
