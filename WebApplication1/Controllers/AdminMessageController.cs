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
        // Queue برای نگهداری عناصر Json
        private static readonly Queue<JsonElement> _messageQueue = new Queue<JsonElement>();

        // تعریف سرویس MessageHub
        private readonly IHubContext<MessageHub> _hubContext;


        public AdminMessageController(IHubContext<MessageHub> hubContext)
        {
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

          
            _messageQueue.Enqueue(jsonElement);

            
            if (hasMessage)
            {
                await _hubContext.Clients.All.SendAsync("Message Received With Message Field", jsonElement.ToString());
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("Message Received Without Message Field", jsonElement.ToString());
            }

            
            if (_messageQueue.Count >= 10)
            {
                var allMessages = _messageQueue.ToArray();

               
                await _hubContext.Clients.All.SendAsync("QueueStatusUpdate", "Queue reached 10 items and will clear after 10 seconds");

              
                await _hubContext.Clients.All.SendAsync("Send All Messages To Clients", JsonSerializer.Serialize(allMessages));

               
                await Task.Delay(10000);

                
                _messageQueue.Clear();
            }

            return Ok(new { Status = "Message sent to client and queued successfully" });
        }
    }
}