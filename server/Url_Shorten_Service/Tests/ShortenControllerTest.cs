using Xunit;
using Moq;
using Url_Shorten_Service.DTOs;
using Url_Shorten_Service.Services;
using Url_Shorten_Service.Controllers;
using Microsoft.AspNetCore.Mvc;
using Url_Shorten_Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Url_Shorten_Service.Tests
{
    public class ShortenControllerTests
    {
        private ShortenDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ShortenDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ShortenDbContext(options);
        }

        [Fact]
        public async Task SendShortUrl_WithInvalidUrl_ShouldReturnBadRequest()
        {
            var context = GetInMemoryDbContext();
            var publishMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishMock.Object);

            var controller = new ShortenController(service);
            var dto = new ShortenDto
            {
                OriginalUrl = "not-a-valid-url",
                ShortenCode = null
            };

            var result = await controller.SendShortUrl(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task SendShortUrl_WithInvalidCodeLength_ShouldReturnBadRequest()
        {
            var context = GetInMemoryDbContext();
            var publishMock = new Mock<IPublishEndpoint>();
            var service = new SendUrlService(context, publishMock.Object);

            var controller = new ShortenController(service);
            var dto = new ShortenDto
            {
                OriginalUrl = "https://www.example.com",
                ShortenCode = "short"
            };

            var result = await controller.SendShortUrl(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }
    }
}
