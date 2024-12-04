using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TransferRequestApprovedEventHandlerForEmailNotifications(IMediator mediator, IEncodingService encodingService) : IHandleMessages<TransferRequestApprovedEvent>
{
    public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
    {
        var cohortSummary = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

        var cohortReference = encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference);

        var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(cohortSummary.AccountId,
            "SenderApprovedCommitmentEmployerNotification", new Dictionary<string, string>
            {
                { "employer_name", cohortSummary.LegalEntityName },
                { "cohort_reference", cohortReference },
                { "sender_name", cohortSummary.TransferSenderName }
            },
            cohortSummary.LastUpdatedByEmployerEmail);

        var sendEmailToProviderCommand = new SendEmailToProviderCommand(cohortSummary.ProviderId.Value,
            "SenderApprovedCommitmentProviderNotification",
            new Dictionary<string, string>
            {
                { "cohort_reference", cohortReference }
            },
            cohortSummary.LastUpdatedByProviderEmail);

        var options = new SendOptions();

        await Task.WhenAll(
            context.Send(sendEmailToProviderCommand, options),
            context.Send(sendEmailToEmployerCommand, options)
        );
    }
}