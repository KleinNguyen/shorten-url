using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Url_Crud_Service.Data;
using Url_Crud_Service.Models;
using Url_Crud_Service.Services;
using Shareds_Events;

namespace Url_Crud_Service.Tests
{
    public class ReceiveUrlServiceTests
    {
        private CrudDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<CrudDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new CrudDbContext(options);
        }

        [Fact]
        public async Task Consume_WithUrlShortenEvent_ShouldSaveUrlCrud()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlService(context);

            var urlSendEvent = new UrlSendEvent
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com/long/url",
                ShortenUrl = "http://localhost:5000/api/shorten/abc1234",
                ShortenCode = "abc1234",
                SourceService = "UrlShorten",
                DateTime = DateTime.UtcNow
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlSendEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(urlSendEvent);

            await consumer.Consume(consumeContextMock.Object);

            var saved = await context.UrlCruds.FirstOrDefaultAsync(u => u.ShortenCode == "abc1234");
            Assert.NotNull(saved);
            Assert.Equal("user@example.com", saved.Email);
            Assert.Equal("https://www.example.com/long/url", saved.OriginalUrl);
            Assert.Equal("abc1234", saved.ShortenCode);
        }

        [Fact]
        public async Task Consume_WithNonUrlShortenSource_ShouldNotSave()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlService(context);

            var urlSendEvent = new UrlSendEvent
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenUrl = "http://localhost:5000/api/shorten/xyz9999",
                ShortenCode = "xyz9999",
                SourceService = "OtherService",
                DateTime = DateTime.UtcNow
            };

            var consumeContextMock = new Mock<ConsumeContext<UrlSendEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(urlSendEvent);

            await consumer.Consume(consumeContextMock.Object);

            var saved = await context.UrlCruds.FirstOrDefaultAsync(u => u.ShortenCode == "xyz9999");
            Assert.Null(saved);
        }

        [Fact]
        public async Task Consume_WithMultipleEvents_ShouldSaveAll()
        {
            var context = GetInMemoryDbContext();
            var consumer = new ReceiveUrlService(context);

            var event1 = new UrlSendEvent
            {
                Email = "user1@example.com",
                OriginalUrl = "https://www.google.com",
                ShortenUrl = "http://localhost:5000/api/shorten/ggl1111",
                ShortenCode = "ggl1111",
                SourceService = "UrlShorten",
                DateTime = DateTime.UtcNow
            };

            var event2 = new UrlSendEvent
            {
                Email = "user2@example.com",
                OriginalUrl = "https://www.github.com",
                ShortenUrl = "http://localhost:5000/api/shorten/ghb2222",
                ShortenCode = "ghb2222",
                SourceService = "UrlShorten",
                DateTime = DateTime.UtcNow
            };

            var contextMock1 = new Mock<ConsumeContext<UrlSendEvent>>();
            contextMock1.Setup(x => x.Message).Returns(event1);

            var contextMock2 = new Mock<ConsumeContext<UrlSendEvent>>();
            contextMock2.Setup(x => x.Message).Returns(event2);

            await consumer.Consume(contextMock1.Object);
            await consumer.Consume(contextMock2.Object);

            var allUrls = await context.UrlCruds.ToListAsync();
            Assert.Equal(2, allUrls.Count);
            Assert.Contains(allUrls, u => u.ShortenCode == "ggl1111");
            Assert.Contains(allUrls, u => u.ShortenCode == "ghb2222");
        }
    }
}
