using Quartz;
using EmailAutomation.API.Models;
using EmailAutomation.API.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EmailAutomation.API.Jobs;

[DisallowConcurrentExecution]
public class DailyEmailJob : IJob
{
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<DailyEmailJob> _logger;

    public DailyEmailJob(IEmailQueue emailQueue, ILogger<DailyEmailJob> logger)
    {
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Daily Email Job Triggered at {Time}", DateTimeOffset.Now);

        try
        {
            // In a real application, you would fetch users or content from a database here.
            // For testing/demonstration, we are queueing a sample email.
            
            var emailRequest = new EmailRequest
            {
                ToEmail = "sample.user@example.com", // Replace with actual recipient
                ToName = "Sample User",
                Subject = "Daily Scheduled Email",
                Body = $"<h1>Your Daily Update</h1><p>This is your scheduled daily email, sent at {DateTime.Now:g}</p>",
                IsHtml = true
            };

            await _emailQueue.EnqueueEmailAsync(emailRequest);
            
            _logger.LogInformation("Daily Email successfully added to queue for {ToEmail}", emailRequest.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing DailyEmailJob.");
        }
    }
}
