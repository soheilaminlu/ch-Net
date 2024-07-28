using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class MessageModel
    {
        public int? Id { get; set; }

        public string? Content { get; set; } = string.Empty;

        
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateModified { get; set; } = DateTime.UtcNow;

        public int? views { get; set; } = 0;

        public bool? published { get; set; } = false;

        public UserModel? User { get; set; }


        // Foreign key
        public int? UserId { get; set; }
    }
}
