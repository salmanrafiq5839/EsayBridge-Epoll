using EPOLL.Website.Infrastructure.Interfaces;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        public Bitmap GenerateQrCode(string input)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }
    }
}
