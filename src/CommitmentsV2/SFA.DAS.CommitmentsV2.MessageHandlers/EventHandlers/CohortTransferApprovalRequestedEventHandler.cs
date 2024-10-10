using SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortTransferApprovalRequestedEventHandler(IMediator mediator, IEncodingService encodingService, ILogger<CohortTransferApprovalRequestedEventHandler> logger)
    : IHandleMessages<CohortTransferApprovalRequestedEvent>
{
    public async Task Handle(CohortTransferApprovalRequestedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{TypeName} processing started.", nameof(CohortTransferApprovalRequestedEventHandler));

        await mediator.Send(new AddTransferRequestCommand { CohortId = message.CohortId, LastApprovedByParty = message.LastApprovedByParty });

        var cohortSummary = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

        if (message.LastApprovedByParty == Party.Employer)
        {
            logger.LogInformation("{TypeName} - Last Approved by Party: Employer.", nameof(CohortTransferApprovalRequestedEventHandler));
        }
        else if (message.LastApprovedByParty == Party.Provider)
        {
            logger.LogInformation("{TypeName} - Last Approved by Party: Provider.", nameof(CohortTransferApprovalRequestedEventHandler));

            var tokens = new Dictionary<string, string>
            {
                { "provider_name", cohortSummary.ProviderName },
                { "sender_name", cohortSummary.TransferSenderName },
                { "employer_hashed_account", encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
                { "cohort_reference", encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference) }
            };

            await context.Send(new SendEmailToEmployerCommand(
                cohortSummary.AccountId,
                "EmployerTransferPendingFinalApproval",
                tokens,
                cohortSummary.LastUpdatedByEmployerEmail)
            );
        }

        logger.LogInformation("{TypeName} processing completed.", nameof(CohortTransferApprovalRequestedEventHandler));
    }
}