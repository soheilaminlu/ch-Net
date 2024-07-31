using WebApplication1.Dto;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Mapper
{
    public static class MessagesMapper
    {
        public static MessagesDto ToMessagesDto(this MessageModel message)
        {
            return new MessagesDto
            {
                Id = message.Id,
                Content = message.Content,
                DateCreated = message.DateCreated,
                DateModified = message.DateModified,
                views = message.views,
                published = message.published,
                UserId = message.UserId
            };
        }

        public static IEnumerable<MessagesDto> ToMessagesDtos(this IEnumerable<MessageModel> messages)
        {
            return messages.Select(m => m.ToMessagesDto());
        }


    }
}