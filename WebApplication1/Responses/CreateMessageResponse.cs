using WebApplication1.Models;
using WebApplication1.Dto;

namespace WebApplication1.TestHelpers
{
    public class CreateMessageResponse
    {
        public string? Message { get; set; }
        public MessagesDto? MessageInfo { get; set; }
    }
}
