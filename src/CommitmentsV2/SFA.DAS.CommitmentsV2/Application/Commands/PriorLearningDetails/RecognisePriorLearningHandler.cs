using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails
{
    public class PriorLearningDetailsHandler : IRequestHandler<PriorLearningDetailsCommand>
    {
        private readonly ILogger<PriorLearningDetailsHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public PriorLearningDetailsHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<PriorLearningDetailsHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(PriorLearningDetailsCommand command, CancellationToken cancellationToken)
        {
            var apprenticeship = await _dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

            if (command.Rpl2Mode)
            {
                apprenticeship.SetPriorLearningDetailsExtended(command.DurationReducedByHours, command.PriceReducedBy);
            }
            else
            {
                apprenticeship.SetPriorLearningDetails(command.DurationReducedBy, command.PriceReducedBy);
            }

            _logger.LogInformation($"Set PriorLearning details set for draft Apprenticeship:{command.ApprenticeshipId}, Rpl Extended Mode: {command.Rpl2Mode}");
        }
    }
}