using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.IOptions
{
    public class IEmailOptions
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
    }
}
