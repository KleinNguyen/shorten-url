using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Url_Shorten_Service.Data;
using Url_Shorten_Service.DTOs;
using Url_Shorten_Service.Models;
using Url_Shorten_Service.Services;
using Shareds_Events;

namespace Url_Shorten_Service.Tests
{
    public class SendUrlServiceTests
    {
        private ShortenDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ShortenDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ShortenDbContext(options);
        }

        [Fact]
        public async Task SendShortUrl_WithValidUrl_ShouldCreateAndReturnShortUrl()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishEndpointMock.Object);

            var dto = new ShortenDto
            {
                OriginalUrl = "https://www.example.com/very/long/url",
                ShortenCode = null
            };
            string baseUrl = "http://localhost:5000";

            var result = await service.SendShortUrl(dto, baseUrl, "user@example.com");

            Assert.NotNull(result);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal(dto.OriginalUrl, result.OriginalUrl);
            Assert.NotNull(result.ShortenCode);
            Assert.Equal(7, result.ShortenCode.Length);

            publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<UrlSendEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendShortUrl_WithCustomCode_ShouldUseProvidedCode()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishEndpointMock.Object);

            var dto = new ShortenDto
            {
                OriginalUrl = "https://www.github.com",
                ShortenCode = "abc1234"
            };
            string baseUrl = "http://localhost:5000";

            var result = await service.SendShortUrl(dto, baseUrl, "user@example.com");

            Assert.NotNull(result);
            Assert.Equal("abc1234", result.ShortenCode);
            Assert.Equal(dto.OriginalUrl, result.OriginalUrl);
        }

        [Fact]
        public async Task SendShortUrl_WithDuplicateCode_ShouldThrowException()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishEndpointMock.Object);

            var existingUrl = new UrlShorten
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenCode = "dup1234",
                DateTime = DateTime.UtcNow
            };
            context.UrlShortenes.Add(existingUrl);
            await context.SaveChangesAsync();

            var dto = new ShortenDto
            {
                OriginalUrl = "https://www.different.com",
                ShortenCode = "dup1234"
            };

            var exception = await Assert.ThrowsAsync<Exception>(
                () => service.SendShortUrl(dto, "http://localhost:5000", "user@example.com"));
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task GetShortUrlByCode_WithValidCode_ShouldReturnUrl()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishEndpointMock.Object);

            var shortUrl = new UrlShorten
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com/page",
                ShortenCode = "test123",
                DateTime = DateTime.UtcNow
            };
            context.UrlShortenes.Add(shortUrl);
            await context.SaveChangesAsync();

            var result = await service.GetShortUrlByCode("test123");

            Assert.NotNull(result);
            Assert.Equal("test123", result.ShortenCode);
            Assert.Equal("https://www.example.com/page", result.OriginalUrl);
        }

        [Fact]
        public async Task GetShortUrlByCode_WithInvalidCode_ShouldReturnNull()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishEndpointMock.Object);

            var result = await service.GetShortUrlByCode("nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public async Task SendShortUrl_WithoutAuthentication_ShouldCreateUrlWithNullEmail()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishEndpointMock.Object);

            var dto = new ShortenDto
            {
                OriginalUrl = "https://www.google.com",
                ShortenCode = null
            };

            var result = await service.SendShortUrl(dto, "http://localhost:5000", null);

            Assert.NotNull(result);
            Assert.Null(result.Email);
            Assert.Equal(dto.OriginalUrl, result.OriginalUrl);
        }
    }
}
