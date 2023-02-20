using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Helpers
{
    public class Strings
    {
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
