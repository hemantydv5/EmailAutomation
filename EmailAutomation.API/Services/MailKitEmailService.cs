using EmailAutomation.API.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace EmailAutomation.API.Services;

public class MailKitEmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IOptions<EmailConfiguration> emailConfig, ILogger<MailKitEmailService> logger)
    {
        _emailConfig = emailConfig.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailConfig.SenderEmail);
            email.Sender.Name = _emailConfig.SenderName;
            
            // For From, we usually use the same as sender
            email.From.Add(new MailboxAddress(_emailConfig.SenderName, _emailConfig.SenderEmail));
            
            email.To.Add(new MailboxAddress(string.IsNullOrEmpty(request.ToName) ? request.ToEmail : request.ToName, request.ToEmail));
            
            email.Subject = request.Subject;

            var builder = new BodyBuilder();
            if (request.IsHtml)
            {
                builder.HtmlBody = request.Body;
            }
            else
            {
                builder.TextBody = request.Body;
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            // Connect to SMTP Server
            var secureSocketOptions = _emailConfig.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            
            // Some servers like standard port 587 with StartTls, or 465 with SslOnConnect
            await smtp.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, secureSocketOptions);
            
            // Authenticate if credentials are provided
            if (!string.IsNullOrEmpty(_emailConfig.Username) && !string.IsNullOrEmpty(_emailConfig.Password))
            {
                await smtp.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {ToEmail}", request.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", request.ToEmail);
            throw; // Re-throw to be handled by caller or queue
        }
    }
}
