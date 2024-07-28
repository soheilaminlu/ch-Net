using WebApplication1.Dto;
using WebApplication1.Models;


// Check inputs for update messages
namespace WebApplication1.Utils
{
    public class ApplyMessageUpdates
    {
        public static class MessageUpdateHelper
        {
            public static void ApplyMessagesUpdate(MessageModel message , UpdateMessageDto updateMessage)
            {
                if (!string.IsNullOrEmpty(updateMessage.Content))
                {
                    message.Content = updateMessage.Content;
                }
            }
        }
    }
}
