using Microsoft.Extensions.Caching.Distributed;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class RunHealthCheckCommandHandler : IHandleMessages<RunHealthCheckCommand>
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RunHealthCheckCommandHandler> _logger;

        public RunHealthCheckCommandHandler(IDistributedCache distributedCache, ILogger<RunHealthCheckCommandHandler> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public Task Handle(RunHealthCheckCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Handled {nameof(RunHealthCheckCommand)} with MessageId '{context.MessageId}'");

            return _distributedCache.SetStringAsync(context.MessageId, "OK");
        }
    }
}