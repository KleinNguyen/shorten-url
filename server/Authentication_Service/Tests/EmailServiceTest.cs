using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Authentication_Service.Services;

namespace Authentication_Service.Tests
{
    public class EmailServiceTests
    {
        private IConfiguration GetMockConfiguration()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Email:SmtpHost"]).Returns("smtp.gmail.com");
            configMock.Setup(x => x["Email:SmtpPort"]).Returns("587");
            configMock.Setup(x => x["Email:Username"]).Returns("test@gmail.com");
            configMock.Setup(x => x["Email:Password"]).Returns("testpassword");
            return configMock.Object;
        }

        [Fact]
        public void EmailService_ShouldInitializeWithConfiguration()
        {
            var configuration = GetMockConfiguration();
            var emailService = new EmailService(configuration);
            Assert.NotNull(emailService);
        }
    }
}
