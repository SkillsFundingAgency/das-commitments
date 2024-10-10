using Microsoft.Extensions.Caching.Distributed;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class RunHealthCheckCommandHandler(IDistributedCache distributedCache, ILogger<RunHealthCheckCommandHandler> logger)
    : IHandleMessages<RunHealthCheckCommand>
{
    public Task Handle(RunHealthCheckCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation("Handled {TypeName} with MessageId '{MessageId}'", nameof(RunHealthCheckCommand), context.MessageId);

        return distributedCache.SetStringAsync(context.MessageId, "OK");
    }
}