using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using App.Core.Interface;

namespace NotificationService.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(to))
            {
                throw new ArgumentNullException(nameof(to), "Recipient email address cannot be null or empty.");
            }

            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = _configuration["Smtp:Port"];
            var smtpEmail = _configuration["Smtp:Email"];
            var smtpPassword = _configuration["Smtp:Password"];
            var smtpEnableSsl = _configuration["Smtp:EnableSsl"];
            var smtpFrom = _configuration["Smtp:From"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || string.IsNullOrEmpty(smtpEmail) ||
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(smtpEnableSsl) || string.IsNullOrEmpty(smtpFrom))
            {
                _logger.LogError("SMTP configuration is missing or invalid.");
                throw new InvalidOperationException("SMTP configuration is missing or invalid.");
            }

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = int.Parse(smtpPort),
                Credentials = new NetworkCredential(smtpEmail, smtpPassword),
                EnableSsl = bool.Parse(smtpEnableSsl),
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpFrom),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Recipient}", to);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending email to {Recipient}: {Message}", to, smtpEx.Message);
                throw new InvalidOperationException($"SMTP error sending email to {to}: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Recipient}: {Message}", to, ex.Message);
                throw new InvalidOperationException($"Unexpected error sending email to {to}: {ex.Message}", ex);
            }
        }
    }
}

