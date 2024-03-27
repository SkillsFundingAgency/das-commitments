using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortApprovedByEmployerEventHandler : IHandleMessages<CohortApprovedByEmployerEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<CohortApprovedByEmployerEventHandler> _logger;

        public CohortApprovedByEmployerEventHandler(IMediator mediator, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<CohortApprovedByEmployerEventHandler> logger)
        {
            _mediator = mediator;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(CohortApprovedByEmployerEvent message, IMessageHandlerContext context)
        {
            try
            {
                var cohort = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

                await _legacyTopicMessagePublisher.PublishAsync(new CohortApprovedByEmployer
                {
                    AccountId = cohort.AccountId,
                    ProviderId = cohort.ProviderId.Value,
                    CommitmentId = message.CohortId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish CohortApprovedByEmployer");
                throw;
            }
        }
    }
}