using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Options;
using CleanArchitecture.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/email")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailQueue _queue;
    private readonly SmtpFromOptions _from;

    public EmailTestController(IEmailQueue queue, IOptions<SmtpFromOptions> from)
    {
        _queue = queue;
        _from = from.Value;
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestEmail()
    {
        await _queue.EnqueueAsync(new QueuedEmail
        {
            ToEmail = _from.AdminEmail,
            FromEmail = _from.FromEmail,
            FromName = _from.FromName,
            Subject = "Mailtrap Test Email",
            HtmlBody = "<h3>Your Mailtrap setup works!</h3>",
            PlainBody = "Your Mailtrap setup works!",
            Type = "test"
        });

        return Ok("Test email queued.");
    }
}
