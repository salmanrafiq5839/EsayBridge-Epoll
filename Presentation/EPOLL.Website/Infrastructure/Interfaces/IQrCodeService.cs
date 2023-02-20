using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Interfaces
{
    public interface IQrCodeService
    {
        Bitmap GenerateQrCode(string input);
    }
}
