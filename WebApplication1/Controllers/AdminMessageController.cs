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

        //Definition MesaageHubService in Service Folder
        private readonly IHubContext<MessageHub> _hubContext;

        // MessageHub Configuration
        public AdminMessageController(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] JsonElement jsonElement)
        {  
           
            bool hasMessage = false;
            // set condition if input has message field
            if (jsonElement.TryGetProperty("message", out var message))
            {
                var messageString = message.GetString();
                if (!string.IsNullOrEmpty(messageString))
                {
                    hasMessage = true;
                }
            }

            // اضافه کردن JSON ورودی به صف
            _messageQueue.Add(jsonElement);

            // اطلاع‌رسانی برای پیام‌های دریافت شده یا صف
            if (hasMessage)
            {
                await _hubContext.Clients.All.SendAsync("Message Received and Queued with message field", jsonElement.ToString());
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("Message Recieved and Queued without message field", jsonElement.ToString());
            }

            // Check Queue Count
            if (_messageQueue.Count >= 10)
            {
                var allMessages = _messageQueue.ToArray();

                // Notification for Queue Status
                await _hubContext.Clients.All.SendAsync("QueueStatusUpdate", "Queue reached 10 items and will clear after 10 seconds");

                // Make sure Messages Recieved to Clients
                await _hubContext.Clients.All.SendAsync("Send All Messages To Clients", allMessages);

                // 10 Second Delay
                await Task.Delay(10000);

              //Clear Queue
                _messageQueue.Clear();
            }

            return Ok(new { Status = "Message sent to client and queued successfully" });
        }
    }
}