using System.ComponentModel.DataAnnotations;
namespace Authentication_Service.Models
{
    public class User
    {
        public int Id {  get; set; }
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public bool ResetTokenVerified { get; set; } = false;

    }
}
