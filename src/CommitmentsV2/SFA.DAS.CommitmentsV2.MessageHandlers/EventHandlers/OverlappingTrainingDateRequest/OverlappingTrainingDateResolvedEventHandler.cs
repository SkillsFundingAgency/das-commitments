using System.Threading;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
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

            var draftApprenticeship = await dbContext.Value.DraftApprenticeships
                .Include(a => a.Cohort).ThenInclude(c => c.Provider)
                .SingleOrDefaultAsync(a => a.Id == message.ApprenticeshipId && a.CommitmentId == message.CohortId, CancellationToken.None);

            if (draftApprenticeship == null)
            {
                logger.LogInformation("Apprenticeship id {ApprenticeshipId} was not found", message?.ApprenticeshipId);
                return;
            }

            if (draftApprenticeship.Cohort.IsApprovedByAllParties)
            {
                logger.LogInformation("Apprenticeship has already been approved for id {ApprenticeshipId}", message?.ApprenticeshipId);
                return;
            }

            var sendEmailToProviderCommand = BuildEmailToEmployerCommand(draftApprenticeship);

            await context.Send(sendEmailToProviderCommand, new SendOptions());
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