using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Interfaces;
using WebApplication1.Mapper;
using WebApplication1.Models;

namespace WebApplication1.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(ApplicationDbContext context , ILogger<MessageRepository> logger)
        {
            _context = context; 
            _logger = logger;
        }
        public async Task<MessagesDto?> CreateMessageAsync(CreateMessageDto messageDto)
        {
            if (messageDto == null)
            {
                _logger.LogInformation("Received null messageDto.");
                return null;
            }

            var message = new MessageModel
            {
                Content = messageDto.Content,
                UserId = messageDto.UserId,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDtoResult = message.ToMessagesDto();
            return messageDtoResult;
        }
        public async Task<MessageModel?> DeleteMessageAsync(int id)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                _logger.LogInformation("null Response For Deleting Message");
                return null;
            }
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<MessagesDto>> GetAllMessagesAsync()
        {
            var messages = await _context.Messages.ToListAsync();
            if (messages == null)
            {
                _logger.LogInformation("null Response For Messages");
                return new List<MessagesDto>(); 
            }

            var messagesDto = messages.ToMessagesDtos().ToList();
            return messagesDto;
        }

        public async Task<MessagesDto?> GetMessageByIdAsync(int id)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
            if(message == null)
            {
                _logger.LogInformation("null Result for Find Messages");
                return null;
            }
            var messsageDtoResult = message.ToMessagesDto();
            return messsageDtoResult;
        }

        public async Task<List<MessagesDto>> GetUserMessagesByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Messages) // Ensure messages are included
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogInformation("User with Id {UserId} not found.", userId);
                return new List<MessagesDto>();
            }

            var userMessages = user.Messages;

            if (userMessages == null || !userMessages.Any())
            {
                _logger.LogInformation("No messages found for User with Id {UserId}.", userId);
                return new List<MessagesDto>();
            }

            var messagesDtoList = userMessages.Select(m => m.ToMessagesDto()).ToList();

            return messagesDtoList;
        }

        public async Task<MessagesDto?> UpdateMessageAsync(int id, UpdateMessageDto messageDto)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                _logger.LogInformation("Not Found Message For Update");
                return null;
            }
            message.Content = messageDto.Content;
            await _context.SaveChangesAsync();
            var messageDtoResult = message.ToMessagesDto();
            return messageDtoResult;
        }
    }
}
