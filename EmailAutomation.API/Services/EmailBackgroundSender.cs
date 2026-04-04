using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailAutomation.API.Services;

public class EmailBackgroundSender : BackgroundService
{
    private readonly IEmailQueue _emailQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundSender> _logger;

    public EmailBackgroundSender(IEmailQueue emailQueue, IServiceProvider serviceProvider, ILogger<EmailBackgroundSender> logger)
    {
        _emailQueue = emailQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Background Sender Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var emailRequest = await _emailQueue.DequeueEmailAsync(stoppingToken);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                
                _logger.LogInformation("Processing email to {ToEmail}", emailRequest.ToEmail);
                await emailService.SendEmailAsync(emailRequest);
                _logger.LogInformation("Email successfully processed for {ToEmail}", emailRequest.ToEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing email send to {ToEmail}", emailRequest.ToEmail);
                // Depending on requirements, we could re-enqueue the email here.
            }
        }

        _logger.LogInformation("Email Background Sender Service is stopping.");
    }
}
