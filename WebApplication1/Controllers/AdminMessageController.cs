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

    public AdminMessageController(IAdminRepository adminRepo, IHubContext<MessageHub> hubContext)
    {
        _adminRepo = adminRepo;
        _hubContext = hubContext;
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
            await _hubContext.Clients.All.SendAsync("Message Received With Message Field", jsonElement.ToString());
        }
        else
        {
            await _hubContext.Clients.All.SendAsync("Message Received Without Message Field", jsonElement.ToString());
        }

        await _adminRepo.ProcessQueueAsync(_hubContext);

        return Ok(new { Status = "Message sent to client and queued successfully" });
    }
}