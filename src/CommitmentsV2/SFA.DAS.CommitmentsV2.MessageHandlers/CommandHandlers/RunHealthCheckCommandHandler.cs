using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class RunHealthCheckCommandHandler : IHandleMessages<RunHealthCheckCommand>
    {
        private readonly ILogger<RunHealthCheckCommandHandler> _logger;

        public RunHealthCheckCommandHandler(ILogger<RunHealthCheckCommandHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(RunHealthCheckCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Handled {nameof(RunHealthCheckCommand)} with MessageId '{context.MessageId}'");

            return Task.CompletedTask;
        }
    }
}