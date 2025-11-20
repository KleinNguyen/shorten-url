using System.ComponentModel.DataAnnotations;

namespace Url_Shorten_Service.DTOs
{
    public class ShortenDto
    {
        
        public string? OriginalUrl { get; set; } 
        public string? ShortenCode { get; set; }

    }
}
