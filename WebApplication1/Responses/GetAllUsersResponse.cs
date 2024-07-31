using WebApplication1.Dto;

namespace WebApplication1.TestHelpers
{
    public class GetAllUsersResponse
    {
        public string? Message { get; set; }
        public IEnumerable<UserDto>? Users { get; set; }
        }
    }
