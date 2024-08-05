using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using WebApplication1.Services;

namespace WebApplication1.Interfaces
{
    public interface IAdminRepository
    {
        void EnqueueMessage(JsonElement jsonElement);
        Task ProcessQueueAsync(IHubContext<MessageHub> hubContext);
        Task ClearQueueAsync();
    }
}
