using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprovedCohortReturnedToProviderEventHandler : IHandleMessages<ApprovedCohortReturnedToProviderEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<ApprovedCohortReturnedToProviderEventHandler> _logger;

        public ApprovedCohortReturnedToProviderEventHandler(IMediator mediator, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprovedCohortReturnedToProviderEventHandler> logger)
        {
            _mediator = mediator;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(ApprovedCohortReturnedToProviderEvent message, IMessageHandlerContext context)
        {
            try
            {
                var cohort = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

                if (cohort == null)
                {
                    _logger.LogInformation("Cohort {cohortId} not found when processing ApprovedCohortReturnedToProviderEvent", message.CohortId);
                    return;
                }

                await _legacyTopicMessagePublisher.PublishAsync(new ApprovedCohortReturnedToProvider
                {
                    AccountId = cohort.AccountId,
                    ProviderId = cohort.ProviderId.Value,
                    CommitmentId = message.CohortId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish ApprovedCohortReturnedToProvider");
                throw;
            }
        }
    }
}