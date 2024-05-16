using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestApprovedEventHandlerForEmailNotifications : IHandleMessages<TransferRequestApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        public TransferRequestApprovedEventHandlerForEmailNotifications(IMediator mediator, IEncodingService encodingService)
        {
            _mediator = mediator;
            _encodingService = encodingService;
        }

        public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
        {
            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            var cohortReference = _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference);

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

            await Task.WhenAll(
                context.Send(sendEmailToProviderCommand, new SendOptions()),
                context.Send(sendEmailToEmployerCommand, new SendOptions())
            );
        }
    }
}
