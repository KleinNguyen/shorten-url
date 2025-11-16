namespace Url_Crud_Service.Models
{
    public class UrlCrud
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortenUrl { get; set; }
        public string ShortenCode { get; set; }
        public DateTime DateTime { get; set; }
    }
}
