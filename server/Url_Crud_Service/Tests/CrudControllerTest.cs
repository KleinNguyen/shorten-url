using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Url_Crud_Service.Data;
using Url_Crud_Service.Models;
using Url_Crud_Service.Controllers;
using MassTransit;
using Shareds_Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Url_Crud_Service.Tests
{
    public class CrudControllerTests
    {
        private CrudDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<CrudDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new CrudDbContext(options);
        }

        private ClaimsPrincipal CreateUserWithEmail(string email)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task GetAll_WithValidUser_ShouldReturnUserUrls()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var userEmail = "user@example.com";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUserWithEmail(userEmail)
                }
            };

            var url1 = new UrlCrud
            {
                Email = userEmail,
                OriginalUrl = "https://www.example1.com",
                ShortenUrl = "http://localhost:5000/api/shorten/url11111",
                ShortenCode = "url11111",
                DateTime = System.DateTime.UtcNow.AddDays(-1)
            };

            var url2 = new UrlCrud
            {
                Email = userEmail,
                OriginalUrl = "https://www.example2.com",
                ShortenUrl = "http://localhost:5000/api/shorten/url22222",
                ShortenCode = "url22222",
                DateTime = System.DateTime.UtcNow
            };

            var otherUserUrl = new UrlCrud
            {
                Email = "other@example.com",
                OriginalUrl = "https://www.other.com",
                ShortenUrl = "http://localhost:5000/api/shorten/oth33333",
                ShortenCode = "oth33333",
                DateTime = System.DateTime.UtcNow
            };

            context.UrlCruds.Add(url1);
            context.UrlCruds.Add(url2);
            context.UrlCruds.Add(otherUserUrl);
            await context.SaveChangesAsync();

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUrls = Assert.IsType<List<UrlCrud>>(okResult.Value);
            Assert.Equal(2, returnedUrls.Count);
            Assert.All(returnedUrls, u => Assert.Equal(userEmail, u.Email));
        }

        [Fact]
        public async Task GetById_WithValidIdAndUser_ShouldReturnUrl()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var userEmail = "user@example.com";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUserWithEmail(userEmail)
                }
            };

            var url = new UrlCrud
            {
                Email = userEmail,
                OriginalUrl = "https://www.example.com",
                ShortenUrl = "http://localhost:5000/api/shorten/abc1234",
                ShortenCode = "abc1234",
                DateTime = System.DateTime.UtcNow
            };
            context.UrlCruds.Add(url);
            await context.SaveChangesAsync();

            var result = await controller.GetById(url.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUrl = Assert.IsType<UrlCrud>(okResult.Value);
            Assert.Equal(url.Id, returnedUrl.Id);
            Assert.Equal(userEmail, returnedUrl.Email);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ShouldReturnNotFound()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var userEmail = "user@example.com";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUserWithEmail(userEmail)
                }
            };

            var result = await controller.GetById(999);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("URL not found", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateCode_WithValidNewCode_ShouldUpdateSuccessfully()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var url = new UrlCrud
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenUrl = "http://localhost:5000/api/shorten/old1111",
                ShortenCode = "old1111",
                DateTime = System.DateTime.UtcNow
            };
            context.UrlCruds.Add(url);
            await context.SaveChangesAsync();

            var result = await controller.UpdateCode(url.Id, "new2222");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<UrlUpdateEvent>(), It.IsAny<System.Threading.CancellationToken>()),
                Times.Once);

            var updated = await context.UrlCruds.FirstOrDefaultAsync(u => u.Id == url.Id);
            Assert.Equal("new2222", updated.ShortenCode);
        }

        [Fact]
        public async Task UpdateCode_WithInvalidCodeLength_ShouldReturnBadRequest()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var url = new UrlCrud
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenUrl = "http://localhost:5000/api/shorten/code1",
                ShortenCode = "code1",
                DateTime = System.DateTime.UtcNow
            };
            context.UrlCruds.Add(url);
            await context.SaveChangesAsync();

            var result = await controller.UpdateCode(url.Id, "short");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateCode_WithDuplicateCode_ShouldReturnBadRequest()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var url1 = new UrlCrud
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example1.com",
                ShortenUrl = "http://localhost:5000/api/shorten/code1111",
                ShortenCode = "code1111",
                DateTime = System.DateTime.UtcNow
            };

            var url2 = new UrlCrud
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example2.com",
                ShortenUrl = "http://localhost:5000/api/shorten/dup22222",
                ShortenCode = "dup2222",
                DateTime = System.DateTime.UtcNow
            };

            context.UrlCruds.Add(url1);
            context.UrlCruds.Add(url2);
            await context.SaveChangesAsync();

            var result = await controller.UpdateCode(url1.Id, url2.ShortenCode);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequest.Value);
            Assert.Contains("already exists", message);
        }

        [Fact]
        public async Task Delete_WithValidId_ShouldDeleteAndPublishEvent()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var url = new UrlCrud
            {
                Email = "user@example.com",
                OriginalUrl = "https://www.example.com",
                ShortenUrl = "http://localhost:5000/api/shorten/del11111",
                ShortenCode = "del11111",
                DateTime = System.DateTime.UtcNow
            };
            context.UrlCruds.Add(url);
            await context.SaveChangesAsync();

            var result = await controller.Delete(url.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("Deleted", okResult.Value!.ToString());

            var deleted = await context.UrlCruds.FirstOrDefaultAsync(u => u.Id == url.Id);
            Assert.Null(deleted);

            publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<UrlDeleteEvent>(), It.IsAny<System.Threading.CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ShouldReturnNotFound()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var result = await controller.Delete(999);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("URL not found", notFoundResult.Value);
        }

        [Fact]
        public void HealthCheck_ShouldReturnHealthy()
        {
            var context = GetInMemoryDbContext();
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            var controller = new CrudController(context, publishEndpointMock.Object);

            var result = controller.HealthCheck();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}
