using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using MassTransit;
using Url_Shorten_Service.Data;
using Url_Shorten_Service.Models;
using Url_Shorten_Service.Services;
using Shareds_Events;

namespace Url_Shorten_Service.Tests
{
    public class ReceiveUrlUpdateServiceTests
    {
        private ShortenDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ShortenDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ShortenDbContext(options);
        }

        [Fact]
        public async Task Consume_WithValidUpdateEvent_ShouldUpdateShortenCode()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlUpdateService(context);

            var existingUrl = new UrlShorten
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenCode = "old1234",
                DateTime = DateTime.UtcNow
            };
            context.UrlShortenes.Add(existingUrl);
            await context.SaveChangesAsync();

            var updateEvent = new UrlUpdateEvent
            {
                OldCode = "old1234",
                NewCode = "new1234",
                SourceService = "OtherService"
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlUpdateEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(updateEvent);

            await consumer.Consume(consumeContextMock.Object);

            var updated = await context.UrlShortenes.FirstOrDefaultAsync(u => u.ShortenCode == "new1234");
            Assert.NotNull(updated);
            Assert.Equal("https://www.example.com", updated.OriginalUrl);
        }

        [Fact]
        public async Task Consume_WithSameSourceService_ShouldIgnoreUpdate()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlUpdateService(context);

            var existingUrl = new UrlShorten
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenCode = "code1234",
                DateTime = DateTime.UtcNow
            };
            context.UrlShortenes.Add(existingUrl);
            await context.SaveChangesAsync();

            var updateEvent = new UrlUpdateEvent
            {
                OldCode = "code1234",
                NewCode = "new9999",
                SourceService = "UrlShorten"
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlUpdateEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(updateEvent);

            await consumer.Consume(consumeContextMock.Object);

            var notUpdated = await context.UrlShortenes.FirstOrDefaultAsync(u => u.ShortenCode == "code1234");
            Assert.NotNull(notUpdated);
        }
    }
}
