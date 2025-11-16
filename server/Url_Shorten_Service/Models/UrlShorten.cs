using System.ComponentModel.DataAnnotations;

namespace Url_Shorten_Service.Models
{
    public class UrlShorten
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortenCode { get; set; }
        public DateTime DateTime { get; set; }
    }
}
