using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class AdminMessageController : ControllerBase
    {
        //messageQueue Must have JsonElements
        private static readonly List<JsonElement> _messageQueue = new List<JsonElement>();

        // Definition of MessageHub Service
        private readonly IHubContext<MessageHub> _hubContext;

        public AdminMessageController(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] JsonElement jsonElement)
        {
            bool hasMessage = false;
            // Check if input JSON contains "message" field
            if (jsonElement.TryGetProperty("message", out var message))
            {
                var messageString = message.GetString();
                if (!string.IsNullOrEmpty(messageString))
                {
                    hasMessage = true;
                }
            }

            // Add the input JSON to the queue
            _messageQueue.Add(jsonElement);

            // Notify about the message received
            if (hasMessage)
            {
                await _hubContext.Clients.All.SendAsync("Message Received With Message Field", jsonElement.ToString());
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("Message Received Without Message Field", jsonElement.ToString());
            }

            // Check if the queue has reached or exceeded 10 items
            if (_messageQueue.Count >= 10)
            {
                var allMessages = _messageQueue.ToArray();

                // Notify about queue status
                await _hubContext.Clients.All.SendAsync("QueueStatusUpdate", "Queue reached 10 items and will clear after 10 seconds");

                // Send all queued messages to clients
                await _hubContext.Clients.All.SendAsync("Send All Messages To Clients", JsonSerializer.Serialize(allMessages));

                // Delay for 10 seconds before clearing the queue
                await Task.Delay(10000);

                // Clear the queue
                _messageQueue.Clear();
            }

            return Ok(new { Status = "Message sent to client and queued successfully" });
        }
    }
}
