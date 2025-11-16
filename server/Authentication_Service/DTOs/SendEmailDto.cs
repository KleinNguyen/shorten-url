using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.DTOs
{
    public class SendEmailDto
    {
        [Required]
        public string Email { get; set; }

    }
}
