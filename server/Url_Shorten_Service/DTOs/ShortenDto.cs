using System.ComponentModel.DataAnnotations;

namespace Url_Shorten_Service.DTOs
{
    public class ShortenDto
    {
        [Required]
        public string OriginalUrl { get; set; }
        public string? ShortenCode { get; set; }

    }
}
