using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData
{
    public class RecognisePriorLearningDataHandler : AsyncRequestHandler<PriorLearningDataCommand>
    {
        private readonly ILogger<RecognisePriorLearningDataHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public RecognisePriorLearningDataHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<RecognisePriorLearningDataHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        protected override async Task Handle(PriorLearningDataCommand command, CancellationToken cancellationToken)
        {
            var apprenticeship = await _dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

            apprenticeship.SetPriorLearningData(command.DurationReducedByHours, command.PriceReducedBy, command.WeightageReducedBy, command.QualificationsForRplReduction, command.ReasonForRplReduction, command.IsDurationReducedByRpl, command.TrainingTotalHours, command.CostBeforeRpl);

            _logger.LogInformation($"Set PriorLearning data draft Apprenticeship:{command.ApprenticeshipId}, Rpl Extended Mode: {command.Rpl2Mode}");
        }
    }
}