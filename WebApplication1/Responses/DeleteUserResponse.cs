using WebApplication1.Models;

namespace WebApplication1.TestHelpers
{
    public class DeleteUserResponse
    {
        public string? Message { get; set; }
        public UserModel? User { get; set; }
    }
}
