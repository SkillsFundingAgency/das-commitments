using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Services;

public class NotifyProviderService(IMessageSession messageSession,
    CommitmentsV2Configuration commitmentsV2Configuration,
    ILogger<NotifyProviderService> logger) : INotifyProviderService
{
    public async Task NotifyProvider(long providerId, string apprenticeshipHashedId, string template, string employerName = null)
    {
        var tokens = new Dictionary<string, string>
            {
                {
                    "url",
                    $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{providerId}/apprentices/{apprenticeshipHashedId}"
                }
            };

        if (employerName is not null)
        {
            tokens.Add("employer", employerName);
        }

        var sendEmailToProviderCommand = new SendEmailToProviderCommand(providerId, template,tokens);

        logger.LogInformation("Sending {Template} email to provider: {ProviderId}", template, providerId);
        await messageSession.Send(sendEmailToProviderCommand);
    }
}