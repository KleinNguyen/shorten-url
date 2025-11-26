using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Authentication_Service.Data;
using Authentication_Service.Controllers;
using Authentication_Service.DTOs;
using Authentication_Service.Models;
using Authentication_Service.Services;
using MassTransit;
using BC = BCrypt.Net.BCrypt;

namespace Authentication_Service.Tests
{
    public class AuthControllerTests
    {
        private AuthenticationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AuthenticationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AuthenticationDbContext(options);
        }

        private IConfiguration GetMockConfiguration()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:SecretKey"]).Returns("ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong!");
            configMock.Setup(x => x["Jwt:Issuer"]).Returns("YourAppName");
            configMock.Setup(x => x["Jwt:Audience"]).Returns("YourAppUsers");
            return configMock.Object;
        }

        private AuthController CreateController(AuthenticationDbContext context)
        {
            var config = GetMockConfiguration();
            var publish = new Mock<IPublishEndpoint>(); 
            return new AuthController(context, config, publish.Object);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var user1 = new User { UserName = "user1", Email = "user1@example.com", PasswordHash = BC.HashPassword("password123") };
            var user2 = new User { UserName = "user2", Email = "user2@example.com", PasswordHash = BC.HashPassword("password456") };

            context.Users.Add(user1);
            context.Users.Add(user2);
            await context.SaveChangesAsync();

            var result = await controller.GetAllUsers();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Register_WithNewUser_ShouldCreateUserSuccessfully()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var registerDto = new UserRegisterDto
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                Password = "securePassword123"
            };

            var result = await controller.Register(registerDto);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusResult.StatusCode);

            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            Assert.NotNull(savedUser);
            Assert.Equal("newuser", savedUser.UserName);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var existingUser = new User { UserName = "existing", Email = "existing@example.com", PasswordHash = BC.HashPassword("password123") };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var registerDto = new UserRegisterDto { UserName = "another", Email = "existing@example.com", Password = "password456" };
            var result = await controller.Register(registerDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("already exists", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var password = "correctPassword123";
            var user = new User { UserName = "testuser", Email = "test@example.com", PasswordHash = BC.HashPassword(password) };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loginDto = new UserLoginDto { Email = "test@example.com", Password = password };
            var result = await controller.Login(loginDto);

            var actionResult = Assert.IsType<ActionResult<TokenDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result!);
            Assert.NotNull(okResult.Value);
            Assert.Contains("token", okResult.Value!.ToString());
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ShouldReturnNotFound()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var loginDto = new UserLoginDto { Email = "nonexistent@example.com", Password = "anypassword" };
            var result = await controller.Login(loginDto);

            var actionResult = Assert.IsType<ActionResult<TokenDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result!);
            Assert.Contains("not found", notFoundResult.Value!.ToString());
        }

        [Fact]
        public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var user = new User { UserName = "testuser", Email = "test@example.com", PasswordHash = BC.HashPassword("correctPassword123") };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "wrongPassword" };
            var result = await controller.Login(loginDto);

            var actionResult = Assert.IsType<ActionResult<TokenDto>>(result);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(actionResult.Result!);
            Assert.Contains("incorrect", unauthorizedResult.Value!.ToString());
        }
    }
}
