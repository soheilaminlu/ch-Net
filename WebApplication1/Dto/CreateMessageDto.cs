using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Dto
{
    public class CreateMessageDto
    {

        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 1000 characters.")]
        public string? Content { get; set; } = string.Empty;

        public int? UserId { get; set; } = 0;

    }
}
