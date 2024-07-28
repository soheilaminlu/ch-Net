using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class UpdateMessageDto
    {
        [Required]
        public string? Content { get; set; }
    }
}
