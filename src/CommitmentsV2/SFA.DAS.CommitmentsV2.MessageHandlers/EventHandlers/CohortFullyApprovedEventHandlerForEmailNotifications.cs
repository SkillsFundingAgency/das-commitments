using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortFullyApprovedEventHandlerForEmailNotifications : IHandleMessages<CohortFullyApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        public CohortFullyApprovedEventHandlerForEmailNotifications(IMediator mediator, IEncodingService encodingService)
        {
            _mediator = mediator;
            _encodingService = encodingService;
        }

        public async Task Handle(CohortFullyApprovedEvent message, IMessageHandlerContext context)
        {
            //employer -> send to provider
            //provider -> send to employer
            //transfer sender -> send to both? or handle when transfer request is approved?
            if (message.LastApprovedBy != Party.Provider) return;

            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            var tokens = new Dictionary<string, string>
            {
                {"cohort_reference", _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference)}
            };

            await context.Send(new SendEmailToEmployerCommand(message.AccountId, "EmployerCohortApproved",
                tokens, cohortSummary.LastUpdatedByEmployerEmail));
        }
    }
}
