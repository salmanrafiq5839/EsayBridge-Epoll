using EPOLL.Website.Infrastructure.Interfaces;
using EPOLL.Website.Infrastructure.IOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Services
{
    public class SmtpService : ISmtpService
    {
       
            private readonly IEmailOptions _emailOptions;
            public SmtpService(IOptions<IEmailOptions> emailOptions)
            {
                _emailOptions = emailOptions.Value;
            }

            public async Task SendEmail(string email, string subject, string message)
            {
                using (var client = new SmtpClient())
                {

                    var credential = new NetworkCredential(_emailOptions.Email,_emailOptions.Password);

                    client.Credentials = credential;
                    client.Host = _emailOptions.Host;
                    client.Port = int.Parse(_emailOptions.Port);
                    client.EnableSsl = true;

                    using (var emailMessage = new MailMessage())
                    {
                        emailMessage.To.Add(new MailAddress(email));
                        emailMessage.From = new MailAddress(_emailOptions.Email);
                        emailMessage.Subject = subject;
                        emailMessage.Body = message;
                        await client.SendMailAsync(emailMessage);
                    }
                }
            }
        
    }
}
