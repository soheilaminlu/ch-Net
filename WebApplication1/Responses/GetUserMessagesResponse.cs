using WebApplication1.Dto;

namespace WebApplication1.TestHelpers
{
    public class GetUserMessagesResponse
    {
       public string? Message { get; set; }
        public IEnumerable<MessagesDto>? UserMessages { get; set; }
    }
}
