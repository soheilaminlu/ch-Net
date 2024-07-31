using WebApplication1.Dto;

namespace WebApplication1.TestHelpers
{
    public class GetUserByIdResponse
    {
        public string? Message { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public int? Age { get; set; }
        public string? Email { get; set; }

        public string? Website { get; set; }
        public List<UserMessageDto>? UserMessages { get; set; }
    }
}
