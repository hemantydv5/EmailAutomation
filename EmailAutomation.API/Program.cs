using EmailAutomation.API.Models;
using EmailAutomation.API.Services;
using EmailAutomation.API.Jobs;
using Quartz;

namespace EmailAutomation.API

{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Email Configuration
            builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));

            // Email Services
            builder.Services.AddTransient<IEmailService, MailKitEmailService>();
            builder.Services.AddSingleton<IEmailQueue>(new EmailQueue(100)); // Max 100 queued emails
            builder.Services.AddHostedService<EmailBackgroundSender>();

            // Quartz Configuration
            builder.Services.AddQuartz(q =>
            {
                var jobKey = new JobKey("DailyEmailJob");
                q.AddJob<DailyEmailJob>(opts => opts.WithIdentity(jobKey));
                
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("DailyEmailJob-trigger")
                    // Run daily at 10:00 AM
                    // To test quickly, you can change this to run every minute: "0 * * ? * *"
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())); 
            });

            // Quartz Hosted Service
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => "Email Automation API Running...");
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
