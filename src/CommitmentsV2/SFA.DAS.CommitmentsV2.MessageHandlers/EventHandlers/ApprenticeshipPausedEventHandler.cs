﻿using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipPausedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprenticeshipPausedEventHandler> logger,
    IEncodingService encodingService,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<ApprenticeshipPausedEvent>
{
    public const string EmailTemplateName = "ProviderApprenticeshipPauseNotification";

    public async Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received {HandlerName} for apprentice {ApprenticeshipId}", nameof(ApprenticeshipPausedEventHandler), message?.ApprenticeshipId);

        if (message != null)
        {
            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

            if (apprenticeship.PaymentStatus != PaymentStatus.Paused)
            {
                logger.LogWarning("Apprenticeship '{ApprenticeshipId}' has a PaymentStatus of '{Status}' which is not Paused. Exiting.",
                    apprenticeship.Id,
                    apprenticeship.PaymentStatus.ToString());

                return;
            }

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeship);

            await context.Send(emailToProviderCommand, new SendOptions());
        }
    }

    private SendEmailToProviderCommand BuildEmailToProviderCommand(Apprenticeship apprenticeship)
    {
        var providerCommitmentsBaseUrl = commitmentsV2Configuration.ProviderCommitmentsBaseUrl.EndsWith('/')
            ? commitmentsV2Configuration.ProviderCommitmentsBaseUrl
            : $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/";

        return new SendEmailToProviderCommand(apprenticeship.Cohort.ProviderId, EmailTemplateName,
            new Dictionary<string, string>
            {
                    { "EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name },
                    { "APPRENTICE", $"{apprenticeship.FirstName} {apprenticeship.LastName}" },
                    { "DATE", apprenticeship.PauseDate?.ToString("dd/MM/yyyy") },
                    { "URL", $"{providerCommitmentsBaseUrl}{apprenticeship.Cohort.ProviderId}/apprentices/{encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}" }
            });
    }
}
