
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class CreateUserDto
    {
        [Required(ErrorMessage ="FirstName is Required")]
        [MaxLength(128, ErrorMessage = "First name can't be longer than 128 characters.")]
        public string? FirstName { get; set; } = string.Empty;

        [MaxLength(128, ErrorMessage = "First name can't be longer than 128 characters.")]
        public string? LastName { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        [Required]
        public int? Age { get; set; }

        public string? Website { get; set; }

    }
}
