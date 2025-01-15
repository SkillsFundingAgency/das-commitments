using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class OverlappingTrainingDateResolvedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<OverlappingTrainingDateResolvedEventHandler> logger,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<OverlappingTrainingDateResolvedEvent>
{
    public async Task Handle(OverlappingTrainingDateResolvedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Received {TypeName} for DraftApprenticeship {ApprenticeshipId}", nameof(OverlappingTrainingDateResolvedEvent), message?.ApprenticeshipId);

            if (message != null)
            {
                var draftApprenticeship = await dbContext.Value.GetOLTDResolvedDraftApprenticeshipAggregate(message.CohortId, message.ApprenticeshipId, default);

                var sendEmailToProviderCommand = BuildEmailToEmployerCommand(draftApprenticeship);

                await context.Send(sendEmailToProviderCommand, new SendOptions());
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.EndsWith("can't be modified"))
        {
            logger.LogError(ex, "Apprenticeship has already been approved for id {ApprenticeshipId}", message?.ApprenticeshipId);
        }
        catch (BadRequestException ex) when (ex.Message.EndsWith("was not found"))
        {
            logger.LogError(ex, "Apprenticeship id {ApprenticeshipId} was not found", message?.ApprenticeshipId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Send message to provider for DraftApprenticeship {ApprenticeshipId}", message?.ApprenticeshipId);
            throw;
        }
    }

    private SendEmailToProviderCommand BuildEmailToEmployerCommand(DraftApprenticeship draftApprenticeship)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(draftApprenticeship.Cohort.ProviderId,
            "ProviderOverlappingTrainingDateClosed",
            new Dictionary<string, string>
            {
                { "CohortReference", draftApprenticeship.Cohort.Reference },
                { "Url", $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{draftApprenticeship.Cohort.ProviderId}/unapproved/{draftApprenticeship.Cohort.Reference}/details" }
            }, draftApprenticeship.Cohort.LastUpdatedByProviderEmail
        );

        return sendEmailToProviderCommand;
    }
}