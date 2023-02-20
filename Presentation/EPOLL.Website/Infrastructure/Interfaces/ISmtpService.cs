using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Interfaces
{
    public interface ISmtpService
    {
        Task SendEmail(string email, string subject, string message);

    }
}
