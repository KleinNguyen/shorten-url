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
    public class ReceiveUrlDeleteServiceTests
    {
        private ShortenDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ShortenDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ShortenDbContext(options);
        }

        [Fact]
        public async Task Consume_WithValidDeleteEvent_ShouldRemoveUrl()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlDeleteService(context);

            var shortUrl = new UrlShorten
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenCode = "del1234",
                DateTime = DateTime.UtcNow
            };
            context.UrlShortenes.Add(shortUrl);
            await context.SaveChangesAsync();

            var deleteEvent = new UrlDeleteEvent
            {
                ShortenCode = "del1234",
                SourceService = "OtherService"
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlDeleteEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(deleteEvent);

            await consumer.Consume(consumeContextMock.Object);

            var deleted = await context.UrlShortenes.FirstOrDefaultAsync(u => u.ShortenCode == "del1234");
            Assert.Null(deleted);
        }

        [Fact]
        public async Task Consume_WithNonExistentCode_ShouldNotThrowException()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlDeleteService(context);

            var deleteEvent = new UrlDeleteEvent
            {
                ShortenCode = "nonexistent",
                SourceService = "OtherService"
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlDeleteEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(deleteEvent);

            await consumer.Consume(consumeContextMock.Object);
        }

        [Fact]
        public async Task Consume_WithSameSourceService_ShouldIgnoreDelete()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlDeleteService(context);

            var shortUrl = new UrlShorten
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenCode = "keep1234",
                DateTime = DateTime.UtcNow
            };
            context.UrlShortenes.Add(shortUrl);
            await context.SaveChangesAsync();

            var deleteEvent = new UrlDeleteEvent
            {
                ShortenCode = "keep1234",
                SourceService = "UrlShorten"
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlDeleteEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(deleteEvent);

            await consumer.Consume(consumeContextMock.Object);

            var notDeleted = await context.UrlShortenes.FirstOrDefaultAsync(u => u.ShortenCode == "keep1234");
            Assert.NotNull(notDeleted);
        }
    }
}
