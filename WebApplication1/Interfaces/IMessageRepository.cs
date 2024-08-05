using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IMessageRepository
    {
        Task<List<MessagesDto>> GetAllMessagesAsync();

        Task<MessagesDto?> GetMessageByIdAsync(int id);

        Task<MessagesDto?> CreateMessageAsync(CreateMessageDto messageDto);
        
        Task<MessagesDto?> UpdateMessageAsync(int id , UpdateMessageDto messageDto);

        Task<MessageModel?> DeleteMessageAsync(int id);

        Task<List<MessagesDto>> GetUserMessagesByIdAsync(int userId);

    }
}
