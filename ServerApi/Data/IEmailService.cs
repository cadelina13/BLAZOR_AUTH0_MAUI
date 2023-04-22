using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Data
{
    public interface IEmailService
    {
        Task SendEmail(string email, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string email, string body)
        {
            try
            {
                var message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(_configuration["GmailService:EmailAddress"]);
                message.To.Add(new MailAddress(email));
                message.Subject = "Reset Password";
                message.IsBodyHtml = true;
                message.Body = body;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_configuration["GmailService:EmailAddress"], _configuration["GmailService:Password"]);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }catch(Exception ex)
            {
                throw;
            }
        }
    }
}
