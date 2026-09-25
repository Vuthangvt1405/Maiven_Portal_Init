using System.Net;
using System.Net.Mail;
using Maiven_Portal_Managment.Configuration;
using Microsoft.Extensions.Options;

namespace Maiven_Portal_Managment.Services;

public interface IEmailSender
{
    Task SendPasswordResetOtpAsync(string recipientEmail, string otp, CancellationToken cancellationToken);
}

public sealed class SmtpEmailSender(IOptions<SmtpOptions> optionsAccessor) : IEmailSender
{
    private readonly SmtpOptions options = optionsAccessor.Value;

    public async Task SendPasswordResetOtpAsync(string recipientEmail, string otp, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Host) || string.IsNullOrWhiteSpace(options.FromAddress))
        {
            throw new InvalidOperationException("SMTP must be configured before password reset emails can be sent.");
        }

        using var message = new MailMessage(
            new MailAddress(options.FromAddress, options.FromName),
            new MailAddress(recipientEmail))
        {
            Subject = "Maiven Portal password reset code",
            Body = $"Your Maiven Portal password reset code is: {otp}\n\nIt expires in 10 minutes. Do not share this code.",
            IsBodyHtml = false
        };
        using var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(options.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(options.Username, options.Password)
        };
        await client.SendMailAsync(message, cancellationToken);
    }
}
