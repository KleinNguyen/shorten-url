using MassTransit;
using Url_Shorten_Service.Data;
using Shareds_Events;
using Microsoft.EntityFrameworkCore;

namespace Url_Shorten_Service.Services
{
    public class ReceiveUrlDeleteService : IConsumer<UrlDeleteEvent>
    {
        private readonly ShortenDbContext _context;

        public ReceiveUrlDeleteService(ShortenDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<UrlDeleteEvent> context)
        {
            var evt = context.Message;

            if (evt.SourceService == "UrlShorten")
                return;

            var existing = await _context.UrlShortenes
                .FirstOrDefaultAsync(u => u.ShortenCode == evt.ShortenCode);

            if (existing != null)
            {
                _context.UrlShortenes.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
