using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class
    TransferRequestRejectedEventHandlerForEmailNotifications(
        IMediator mediator,
        IEncodingService encodingService,
        CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<TransferRequestRejectedEvent>
{
    public async Task Handle(TransferRequestRejectedEvent message, IMessageHandlerContext context)
    {
        var cohortSummary = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

        var cohortReference = encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference);

        var employerEncodedAccountId = encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId);

        var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(cohortSummary.AccountId,
            "SenderRejectedCommitmentEmployerNotification", new Dictionary<string, string>
            {
                {"employer_name", cohortSummary.LegalEntityName},
                {"cohort_reference", cohortReference},
                {"sender_name", cohortSummary.TransferSenderName},
                {"RequestUrl", $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}{employerEncodedAccountId}/unapproved/{cohortReference}" }
            },
            cohortSummary.LastUpdatedByEmployerEmail);

        var sendEmailToProviderCommand = new SendEmailToProviderCommand(cohortSummary.ProviderId.Value,
            "SenderRejectedCommitmentProviderNotification",
            new Dictionary<string, string>
            {
                {"cohort_reference", cohortReference},
                {"RequestUrl", $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{cohortSummary.ProviderId.Value}/unapproved/{cohortReference}/details" }
            },
            cohortSummary.LastUpdatedByProviderEmail);

        var options = new SendOptions();
        
        await Task.WhenAll(
            context.Send(sendEmailToProviderCommand, options),
            context.Send(sendEmailToEmployerCommand, options)
        );
    }
}