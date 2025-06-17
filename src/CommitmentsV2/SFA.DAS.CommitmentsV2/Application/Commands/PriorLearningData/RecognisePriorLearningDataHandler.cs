using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData;

public class RecognisePriorLearningDataHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    RplSettingsConfiguration config,
    ILogger<RecognisePriorLearningDataHandler> logger)
    : IRequestHandler<PriorLearningDataCommand>
{
    public async Task Handle(PriorLearningDataCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        apprenticeship.SetPriorLearningData(
            command.TrainingTotalHours,
            command.DurationReducedByHours,
            command.IsDurationReducedByRpl,
            command.DurationReducedBy,
            command.PriceReducedBy,
            config.MinimumPriceReduction,
            config.MaximumTrainingTimeReduction,
            command.MinimumOffTheJobTrainingHoursRequired
        );

        logger.LogInformation("Set PriorLearning data draft Apprenticeship:{ApprenticeshipId}.", command.ApprenticeshipId);
    }
}