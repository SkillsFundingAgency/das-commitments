using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Services;

public class NotifyProviderService(IMessageHandlerContext messageHandlerContext,
    CommitmentsV2Configuration commitmentsV2Configuration,
    ILogger<NotifyProviderService> logger):INotifyProviderService
{
    public async Task NotifyProvider(long providerId, long apprenticeshipId, string providerName, string template)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(providerId, template,
            new Dictionary<string, string>
            {
                {"provider_name", providerName},
                {
                    "URL",
                    $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{providerId}/apprentices/{apprenticeshipId}"
                }
            });

        logger.LogInformation("Sending email to provider: {ProviderId}", providerId);
        await messageHandlerContext.Send(sendEmailToProviderCommand);
    }
}