namespace Shareds_Events;
public class UrlSendEvent
{
    public string? Email { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortenUrl { get; set; }
    public string ShortenCode { get; set; }
    public string SourceService { get; set; } = "UrlShorten";
    public DateTime DateTime { get; set; }
}
