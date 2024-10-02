using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipWithChangeOfPartyCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ApprenticeshipWithChangeOfPartyCreatedEventHandler> logger)
    : IHandleMessages<ApprenticeshipWithChangeOfPartyCreatedEvent>
{
    public async Task Handle(ApprenticeshipWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("ApprenticeshipWithChangeOfPartyCreatedEvent received for Apprenticeship {ApprenticeshipId}, ChangeOfPartyRequest {ChangeOfPartyRequestId}", message.ApprenticeshipId, message.ChangeOfPartyRequestId);

        try
        {
            var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

            if (changeOfPartyRequest.NewApprenticeshipId.HasValue)
            {
                logger.LogWarning("ChangeOfPartyRequest {Id} already has NewApprenticeshipId {CohortId} - {TypeName} with new ApprenticeshipId {ApprenticeshipId} will be ignored",
                    changeOfPartyRequest.Id,
                    changeOfPartyRequest.CohortId,
                    nameof(ApprenticeshipWithChangeOfPartyCreatedEvent),
                    message.ApprenticeshipId
                );
                return;
            }

            changeOfPartyRequest.SetNewApprenticeship(apprenticeship, message.UserInfo, message.LastApprovedBy);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing ApprenticeshipWithChangeOfPartyCreatedEvent");
            throw;
        }
    }
}