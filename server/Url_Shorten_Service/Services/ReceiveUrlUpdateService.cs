using MassTransit;
using Url_Shorten_Service.Data;
using MassTransit;
using Shareds_Events;
using Microsoft.EntityFrameworkCore;

namespace Url_Shorten_Service.Services
{
    public class ReceiveUrlUpdateService : IConsumer<UrlUpdateEvent>
    {
        private readonly ShortenDbContext _context;

        public ReceiveUrlUpdateService(ShortenDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<UrlUpdateEvent> context)
        {
            var evt = context.Message;

            if (evt.SourceService == "UrlShorten")
                return;

            var existing = await _context.UrlShortenes
                .FirstOrDefaultAsync(u => u.ShortenCode == evt.OldCode);

            if (existing != null)
            {
                existing.ShortenCode = evt.NewCode;
                //existing.SHo = evt.ShortenUrl;
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
