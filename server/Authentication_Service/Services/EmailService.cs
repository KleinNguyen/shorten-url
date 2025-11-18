// using Microsoft.Extensions.Configuration;
// using System.Net;
// using System.Net.Mail;
// using System.Threading.Tasks;

// namespace Authentication_Service.Services
// {
//     public class EmailService
//     {
//         private readonly IConfiguration _config;

//         public EmailService(IConfiguration config)
//         {
//             _config = config;
//         }

//         public async Task SendEmailAsync(string toEmail, string subject, string message)
//         {
//             var smtp = new SmtpClient("smtp.gmail.com")
//             {
//                 Port = 587,
//                 Credentials = new NetworkCredential(
//                     _config["Email:Username"],
//                     _config["Email:Password"]),
//                 EnableSsl = true
//             };

//             var mail = new MailMessage
//             {
//                 From = new MailAddress(_config["Email:Username"]),
//                 Subject = subject,
//                 Body = message,
//                 IsBodyHtml = false
//             };

//             mail.To.Add(toEmail);
//             await smtp.SendMailAsync(mail);
//         }
//     }
// }


using Microsoft.Extensions.Configuration;
using System;
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
            // Lấy cấu hình SMTP, ưu tiên ENV VAR
            string smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") 
                                ?? _config["Email:SmtpHost"] 
                                ?? "smtp.gmail.com";

            int smtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) 
                                ? port 
                                : int.TryParse(_config["Email:SmtpPort"], out var port2) ? port2 : 587;

            string emailUser = Environment.GetEnvironmentVariable("EMAIL_USER") 
                                ?? _config["Email:Username"];

            string emailPass = Environment.GetEnvironmentVariable("EMAIL_PASS") 
                                ?? _config["Email:Password"];

            using var smtp = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(emailUser, emailPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(emailUser),
                Subject = subject,
                Body = message,
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);

            try
            {
                await smtp.SendMailAsync(mail);
                Console.WriteLine($"Email sent to {toEmail}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw; // hoặc handle lỗi theo nhu cầu
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                throw;
            }
        }
    }
}
