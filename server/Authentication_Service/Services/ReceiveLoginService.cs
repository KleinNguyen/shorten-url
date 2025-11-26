using MassTransit;
using Authentication_Service.Data;
using Authentication_Service.Models;
using Shareds_Events;

namespace Authentication_Service.Services
{
    public class ReceiveLoginService : IConsumer<LoginEvent>
    {
        private readonly AuthenticationDbContext _context;

        public ReceiveLoginService(AuthenticationDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<LoginEvent> context)
        {
            var evt = context.Message;

            if (evt.SourceService != "Login")
                return;

            var log = new LoginHistory
            {
                UserId = evt.Id,
                Email = evt.Email,
                UserName = evt.UserName,
                LoginTime = DateTime.UtcNow
            };

            _context.LoginHistory.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
