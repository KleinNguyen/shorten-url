namespace Url_Crud_Service.Events
{
    public class UrlUpdateEvent
    {
        public int Id { get; set; }
        public string OldCode { get; set; } = string.Empty;
        public string NewCode { get; set; } = string.Empty;
        public string SourceService { get; set; } = "UrlCrud";
    }
}
