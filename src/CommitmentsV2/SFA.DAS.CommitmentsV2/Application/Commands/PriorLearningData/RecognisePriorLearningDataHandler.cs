using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData
{
    public class RecognisePriorLearningDataHandler : AsyncRequestHandler<PriorLearningDataCommand>
    {
        private readonly ILogger<RecognisePriorLearningDataHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly RplSettingsConfiguration _config;

        public RecognisePriorLearningDataHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            RplSettingsConfiguration config,
            ILogger<RecognisePriorLearningDataHandler> logger)
        {
            _dbContext = dbContext;
            _config = config;
            _logger = logger;
        }

        protected override async Task Handle(PriorLearningDataCommand command, CancellationToken cancellationToken)
        {
            var apprenticeship = await _dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

            apprenticeship.SetPriorLearningData(command.TrainingTotalHours, command.DurationReducedByHours, command.IsDurationReducedByRpl, command.DurationReducedBy, command.PriceReducedBy, _config.MinimumPriceReduction);

            _logger.LogInformation($"Set PriorLearning data draft Apprenticeship:{command.ApprenticeshipId}");
        }
    }
}