namespace Url_Crud_Service.Events;

public class UrlDeleteEvent
{
    public int Id { get; set; }
    public string ShortenCode { get; set; } = string.Empty;
    public string SourceService { get; set; } = "UrlCrud";
}
