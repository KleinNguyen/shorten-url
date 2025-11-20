using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }
        [Required]
        [MinLength(6)]
        public required string ConfirmPassword { get; set; }

    }
}
