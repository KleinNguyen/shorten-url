using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Authentication_Service.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _config["Email:nguyenhuong150905@gmail.com"],   
                    _config["Email:mbro tdib xond gfnh"]),  
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:nguyenhuong150905@gmail.com"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);
            await smtp.SendMailAsync(mail);
        }
    }
}
