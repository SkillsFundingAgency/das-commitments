using System.Threading;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipUpdatedApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprenticeshipUpdatedApprovedEventHandler> logger)
    : IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
{
    public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("ApprenticeshipUpdatedApprovedEvent received for apprenticeshipId: {Id}", message.ApprenticeshipId);
        try
        {
            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, CancellationToken.None);

            await legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateAccepted
            {
                AccountId = apprenticeship.Cohort.EmployerAccountId,
                ProviderId = apprenticeship.Cohort.ProviderId,
                ApprenticeshipId = message.ApprenticeshipId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateAccepted");
            throw;
        }
    }
}