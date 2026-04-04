using System.ComponentModel.DataAnnotations;

namespace EmailAutomation.API.Models;

public class EmailRequest
{
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;

    public string ToName { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    // Optional Attachments could be added here later
}
