using EmailAutomation.API.Models;
using System.Threading.Tasks;

namespace EmailAutomation.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailRequest request);
}
