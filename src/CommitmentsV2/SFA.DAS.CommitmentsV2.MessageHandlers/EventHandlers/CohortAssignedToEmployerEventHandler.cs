using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortAssignedToEmployerEventHandler : IHandleMessages<CohortAssignedToEmployerEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        public CohortAssignedToEmployerEventHandler(IMediator mediator, IEncodingService encodingService)
        {
            _mediator = mediator;
            _encodingService = encodingService;
        }

        public async Task Handle(CohortAssignedToEmployerEvent message, IMessageHandlerContext context)
        {
            if(message.AssignedBy != Party.Provider)  return;

            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            if (cohortSummary == null) { return; }

            if (cohortSummary.ChangeOfPartyRequestId.HasValue) return;

            var tokens = new Dictionary<string, string>
            {
                {"provider_name", cohortSummary.ProviderName },
                {"employer_hashed_account", _encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
                {"cohort_reference", _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference)}
            };

            await context.Send(new SendEmailToEmployerCommand(cohortSummary.AccountId,
                "EmployerCohortNotification",
                tokens, cohortSummary.LastUpdatedByEmployerEmail));
        }
    }
}
