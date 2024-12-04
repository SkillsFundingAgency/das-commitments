using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortAssignedToEmployerEventHandler(
    IMediator mediator,
    IEncodingService encodingService,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<CohortAssignedToEmployerEvent>
{
    public async Task Handle(CohortAssignedToEmployerEvent message, IMessageHandlerContext context)
    {
        if (message.AssignedBy != Party.Provider) return;

        var cohortSummary = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

        if (cohortSummary == null)
        {
            return;
        }

        if (cohortSummary.ChangeOfPartyRequestId.HasValue) return;

        var tokens = new Dictionary<string, string>
        {
            { "provider_name", cohortSummary.ProviderName },
            { "employer_hashed_account", encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
            { "cohort_reference", encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference) },
            { "base_url", commitmentsV2Configuration.EmployerCommitmentsBaseUrl }
        };

        await context.Send(new SendEmailToEmployerCommand(
            cohortSummary.AccountId,
            "EmployerCohortNotification",
            tokens, cohortSummary.LastUpdatedByEmployerEmail)
        );
    }
}