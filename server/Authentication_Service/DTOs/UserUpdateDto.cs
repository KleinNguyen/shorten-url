using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.DTOs
{
    public class UserUpdateDto
    {

        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

    }
}
