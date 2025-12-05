using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string fromName, string fromEmail, string toEmail, string subject, string htmlMessage, string plainTextMessage = "")
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("toEmail is required", nameof(toEmail));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName ?? _options.FromName ?? "", fromEmail ?? _options.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject ?? "";

            var bodyBuilder = new BodyBuilder();

            if (!string.IsNullOrWhiteSpace(htmlMessage))
                bodyBuilder.HtmlBody = htmlMessage;

            if (!string.IsNullOrWhiteSpace(plainTextMessage))
                bodyBuilder.TextBody = plainTextMessage;
            else if (!string.IsNullOrWhiteSpace(htmlMessage))
                bodyBuilder.TextBody = HtmlToPlainText(htmlMessage);

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // connect
                var secureSocket = _options.UseSsl ? SecureSocketOptions.SslOnConnect :
                                  _options.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

                // If port = 0, use default (587)
                var port = _options.Port > 0 ? _options.Port : 587;

                await client.ConnectAsync(_options.Host, port, secureSocket);

                if (!string.IsNullOrWhiteSpace(_options.UserName))
                {
                    await client.AuthenticateAsync(_options.UserName, _options.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }

        private string HtmlToPlainText(string html)
        {
            // basic conversion — for production consider a library, but avoid heavy deps here
            var plain = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
            plain = System.Net.WebUtility.HtmlDecode(plain);
            return plain;
        }
    }
}
