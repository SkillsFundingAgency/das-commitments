using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks;

public class NServiceBusHealthCheck(IMessageSession messageSession, IDistributedCache distributedCache)
    : IHealthCheck
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var messageId = Guid.NewGuid();
        var data = new Dictionary<string, object> { ["MessageId"] = messageId };
        var sendOptions = new SendOptions();
        var stopwatch = Stopwatch.StartNew();
            
        sendOptions.SetMessageId(messageId.ToString());
            
        await messageSession.Send(new RunHealthCheckCommand(), sendOptions);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (await distributedCache.GetStringAsync(messageId.ToString(), cancellationToken) != null)
            {
                return HealthCheckResult.Healthy(null, data);
            }

            if (stopwatch.Elapsed > Timeout)
            {
                return HealthCheckResult.Degraded($"Potential issue with receiving endpoint (failed to handle {nameof(RunHealthCheckCommand)} in sufficient time)", null, data);
            }
                
            await Task.Delay(Interval, cancellationToken);
        }
            
        throw new OperationCanceledException(cancellationToken);
    }
}