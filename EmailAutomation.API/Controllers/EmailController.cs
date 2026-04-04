using EmailAutomation.API.Models;
using EmailAutomation.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EmailAutomation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IEmailQueue _emailQueue;

    public EmailController(IEmailService emailService, IEmailQueue emailQueue)
    {
        _emailService = emailService;
        _emailQueue = emailQueue;
    }

    /// <summary>
    /// Sends an email immediately (Synchronous)
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _emailService.SendEmailAsync(request);
            return Ok(new { message = "Email sent successfully." });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error while sending email", error = ex.Message });
        }
    }

    /// <summary>
    /// Queues an email to be sent in the background (Asynchronous / Automation)
    /// </summary>
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleEmail([FromBody] EmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _emailQueue.EnqueueEmailAsync(request);
        return Accepted(new { message = "Email added to background queue successfully." });
    }
}
