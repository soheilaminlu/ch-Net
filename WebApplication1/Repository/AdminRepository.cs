using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using WebApplication1.Interfaces;
using WebApplication1.Services;

namespace WebApplication1.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private static readonly Queue<JsonElement> _messageQueue = new Queue<JsonElement>();
        public Task ClearQueueAsync()
        {
            _messageQueue.Clear();
            return Task.CompletedTask;
        }

        public void EnqueueMessage(JsonElement jsonElement)
        {
            _messageQueue.Enqueue(jsonElement);
        }

        public async Task ProcessQueueAsync(IHubContext<MessageHub> hubContext)
        {

            if (_messageQueue.Count >= 10)
            {
                var allMessages = _messageQueue.ToArray();

                await hubContext.Clients.All.SendAsync("QueueStatusUpdate", "Queue reached 10 items and will clear after 10 seconds");
                await hubContext.Clients.All.SendAsync("Send All Messages To Clients", JsonSerializer.Serialize(allMessages));

                await Task.Delay(10000);
                _messageQueue.Clear();
            }
        }
    }
}
