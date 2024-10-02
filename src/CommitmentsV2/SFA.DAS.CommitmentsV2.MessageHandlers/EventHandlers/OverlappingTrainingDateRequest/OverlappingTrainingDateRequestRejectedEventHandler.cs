using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using System.Threading;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class OverlappingTrainingDateRequestRejectedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<OverlappingTrainingDateRequestRejectedEventHandler> logger,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<OverlappingTrainingDateRequestRejectedEvent>
{
    public async Task Handle(OverlappingTrainingDateRequestRejectedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received {TypeName} for overlapping training date request {OverlappingTrainingDateRequestId}", nameof(OverlappingTrainingDateRequestRejectedEvent), message.OverlappingTrainingDateRequestId);

        var request = await dbContext.Value.OverlappingTrainingDateRequests
            .Include(r => r.DraftApprenticeship)
            .ThenInclude(d => d.Cohort)
            .Include(r => r.PreviousApprenticeship)
            .ThenInclude(a => a.Cohort)
            .SingleOrDefaultAsync(c => c.Id == message.OverlappingTrainingDateRequestId
                , CancellationToken.None);
        
        var emailToProviderCommand = BuildEmailToProviderCommand(request);

        await context.Send(emailToProviderCommand, new SendOptions());
    }

    private SendEmailToProviderCommand BuildEmailToProviderCommand(Models.OverlappingTrainingDateRequest oltd)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(oltd.DraftApprenticeship.Cohort.ProviderId,
            "ProviderOverlappingTrainingDateRequestRejected",
            new Dictionary<string, string>
            {
                {"CohortReference", oltd.DraftApprenticeship.Cohort.Reference},
                {"URL", $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{oltd.DraftApprenticeship.Cohort.ProviderId}/unapproved/{oltd.DraftApprenticeship.Cohort.Reference}/details" }
            }, oltd.DraftApprenticeship.Cohort.LastUpdatedByProviderEmail);

        return sendEmailToProviderCommand;
    }
}