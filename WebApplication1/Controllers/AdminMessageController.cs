using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Interfaces;
using WebApplication1.Services;

[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigins")]
[ApiController]
public class AdminMessageController : ControllerBase
{
    private readonly IAdminRepository _adminRepo;
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly ILogger<AdminMessageController> _logger;

    public AdminMessageController(IAdminRepository adminRepo, IHubContext<MessageHub> hubContext, ILogger<AdminMessageController> logger)
    {
        _adminRepo = adminRepo;
        _hubContext = hubContext;
        _logger = logger;

    }

    [HttpPost]
    public async Task<IActionResult> PostMessage([FromBody] JsonElement jsonElement)
    {
        bool hasMessage = false;

        if (jsonElement.TryGetProperty("message", out var message))
        {
            var messageString = message.GetString();
            if (!string.IsNullOrEmpty(messageString))
            {
                hasMessage = true;
            }
        }

        _adminRepo.EnqueueMessage(jsonElement);
        

        if (hasMessage)
        {
            _logger.LogInformation("Message Add to queue with message field");
            await _hubContext.Clients.All.SendAsync("Message Received With Message Field", jsonElement.ToString());
        }
        else
        {
            _logger.LogInformation("message Added to queue without message field");
            await _hubContext.Clients.All.SendAsync("Message Received Without Message Field", jsonElement.ToString());
        }

        await _adminRepo.ProcessQueueAsync(_hubContext);
        _logger.LogInformation("Message Recieved to Client Successfuly");
        return Ok(new { Status = "Message sent to client and queued successfully" });
    }
}