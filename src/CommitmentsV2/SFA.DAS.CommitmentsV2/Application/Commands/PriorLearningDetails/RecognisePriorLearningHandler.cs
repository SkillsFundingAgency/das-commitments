using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails;

public class PriorLearningDetailsHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<PriorLearningDetailsHandler> logger)
    : IRequestHandler<PriorLearningDetailsCommand>
{
    public async Task Handle(PriorLearningDetailsCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        if (command.Rpl2Mode)
        {
            apprenticeship.SetPriorLearningDetailsExtended(command.DurationReducedByHours, command.PriceReducedBy);
        }
        else
        {
            apprenticeship.SetPriorLearningDetails(command.DurationReducedBy, command.PriceReducedBy);
        }

        logger.LogInformation("Set PriorLearning details set for draft Apprenticeship:{ApprenticeshipId}, Rpl Extended Mode: {Rpl2Mode}", command.ApprenticeshipId, command.Rpl2Mode);
    }
}