using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.CommitmentsV2.Test;

namespace SFA.DAS.CommitmentsV2.MessageHandlers
{
    public class MessageHandlerService : IHostedService, IDisposable
    {
        private readonly IOptions<MyClass> _options;
        private readonly ILogger<MessageHandlerService> _logger;
        private readonly Interface1 _itest;
        private Timer _timer;

        public MessageHandlerService(IOptions<MyClass> options, ILogger<MessageHandlerService> logger, Interface1 itest)
        {
            _options = options;
            _logger = logger;
            _itest = itest;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Timed Background Service is starting.");
            _logger.LogInformation("hekko");
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Console.WriteLine("Timed Background Service is working.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Timed Background Service is stopping.2");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
