using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.DTOs
{
    public class VerifyCodeDto
    {
        [Required]

        public string Email { get; set; }
        [Required]

        public string Code { get; set; }
    }
}
