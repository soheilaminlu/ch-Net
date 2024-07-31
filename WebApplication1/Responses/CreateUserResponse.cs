using WebApplication1.Models;
using WebApplication1.Dto;

namespace WebApplication1.TestHelpers
{
    public class CreateUserResponse
    {
        public string? Message { get; set; }
        public UserDto? User { get; set; }

    }
}
