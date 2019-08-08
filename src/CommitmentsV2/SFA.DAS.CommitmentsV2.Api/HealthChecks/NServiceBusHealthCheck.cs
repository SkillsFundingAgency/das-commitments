using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks
{
    public class NServiceBusHealthCheck : IHealthCheck
    {
        private readonly IMessageSession _messageSession;

        public NServiceBusHealthCheck(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var messageId = Guid.NewGuid();
            var data = new Dictionary<string, object> { ["MessageId"] = messageId };
            var sendOptions = new SendOptions();
            
            sendOptions.SetMessageId(messageId.ToString());
            
            await _messageSession.Send(new RunHealthCheckCommand(), sendOptions);

            return HealthCheckResult.Healthy(null, data);
        }
    }
}