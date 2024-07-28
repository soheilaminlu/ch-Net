using WebApplication1.Dto;

namespace WebApplication1.TestHelpers
{
    public class GetAllMessagesResponse
    {
        public string? Message {  get; set; }
        public List<MessagesDto>? MessagesInfo { get; set; }
    }
}
