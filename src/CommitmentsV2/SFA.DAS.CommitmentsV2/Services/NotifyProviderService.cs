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
    public async Task NotifyProvider(long providerId, string apprenticeshipHashedId, string template)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(providerId, template,
            new Dictionary<string, string>
            {
                {
                    "URL",
                    $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{providerId}/apprentices/{apprenticeshipHashedId}"
                }
            });

        logger.LogInformation("Sending {Template} email to provider: {ProviderId}", template, providerId);
        await messageSession.Send(sendEmailToProviderCommand);
    }
}