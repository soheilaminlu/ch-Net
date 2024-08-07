using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.TestHelpers
{
    public class DeleteUserResponse
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
