using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;

public class RecognisePriorLearningHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<RecognisePriorLearningHandler> logger)
    : IRequestHandler<RecognisePriorLearningCommand>
{
    public async Task Handle(RecognisePriorLearningCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        apprenticeship.SetRecognisePriorLearning(command.RecognisePriorLearning);

        logger.LogInformation("Set RecognisePriorLearning to {RecognisePriorLearning} for draft Apprenticeship:{ApprenticeshipId}", command.RecognisePriorLearning, command.ApprenticeshipId);
    }
}