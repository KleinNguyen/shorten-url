using MassTransit;
using Url_Crud_Service.Data;
using Url_Crud_Service.Models;
using Shareds_Events;


namespace Url_Crud_Service.Services
{
    public class ReceiveUrlService : IConsumer<UrlSendEvent>
    {
        private readonly CrudDbContext _context;

        public ReceiveUrlService(CrudDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<UrlSendEvent> context)
        {
            var evt = context.Message;

            if (evt.SourceService != "UrlShorten")
                return;

            var detail = new UrlCrud
            {
                Email = evt.Email,
                OriginalUrl = evt.OriginalUrl,
                ShortenUrl = evt.ShortenUrl,
                ShortenCode = evt.ShortenCode,
                DateTime = evt.DateTime
            };

            _context.UrlCruds.Add(detail);
            await _context.SaveChangesAsync();
        }
    }
}
