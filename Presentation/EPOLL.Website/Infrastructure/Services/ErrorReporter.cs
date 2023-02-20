using EPOLL.Website.Infrastructure.Interfaces;
using EPOLL.Website.Infrastructure.IOptions;
using Microsoft.Extensions.Options;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPOLL.Website.Infrastructure.Services
{
    public class ErrorReporter : IErrorReporter
    {
        //private readonly IRavenClient _client;
        private readonly Infrastructure.IOptions.SentryOptions _sentryOptions;

        public ErrorReporter(IOptions<Infrastructure.IOptions.SentryOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.Value.Dsn))
                throw new ArgumentNullException("Can not construct a SentryErrorReporter without a valid DSN!");
            _sentryOptions = options.Value;
        }

        /// <summary>
        ///     Captures the specified exception asynchronously and hands it off to an error handling service.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">exception</exception>
        public async Task CaptureAsync(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            using (SentrySdk.Init(_sentryOptions.Dsn))
            {
                SentrySdk.CaptureException(exception);
            }
            //await _client.CaptureAsync(new SentryEvent(exception));
        }

        /// <summary>
        ///     Captures the specified message asynchronously and hands it off to an error handling service.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
        public async Task CaptureAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));
            using (SentrySdk.Init(_sentryOptions.Dsn))
            {
                SentrySdk.CaptureException(new Exception(message));
            }
        }
    }
}
