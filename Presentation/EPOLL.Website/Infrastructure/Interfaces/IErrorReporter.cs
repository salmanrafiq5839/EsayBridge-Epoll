using System;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Interfaces
{
    public interface IErrorReporter
    {
        Task CaptureAsync(Exception exception);
        Task CaptureAsync(string message);
    }
}
