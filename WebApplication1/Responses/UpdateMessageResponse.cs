using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.TestHelpers
{
    public class UpdateMessageResponse
    {
        public string? Message { get; set; }
        public MessagesDto? UpdatedMessage { get; set; }
    }
}
