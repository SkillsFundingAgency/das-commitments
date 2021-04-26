using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipUpdatedApprovedEventHandler : IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<CohortApprovedByEmployerEventHandler> _logger;

        public ApprenticeshipUpdatedApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<CohortApprovedByEmployerEventHandler> logger)
        {
            _dbContext = dbContext;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            try
            {
               var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, CancellationToken.None);

                await _legacyTopicMessagePublisher.PublishAsync(new ApprenticeshipUpdateAccepted
                {
                    AccountId = apprenticeship.Cohort.EmployerAccountId,
                    ProviderId = apprenticeship.Cohort.ProviderId,
                    ApprenticeshipId = message.ApprenticeshipId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish ApprenticeshipUpdateAccepted");
                throw;
            }
        }
    }
}