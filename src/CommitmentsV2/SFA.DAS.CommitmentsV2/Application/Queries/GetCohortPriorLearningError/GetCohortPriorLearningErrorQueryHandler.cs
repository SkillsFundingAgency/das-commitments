using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortPriorLearningError;

public class GetCohortPriorLearningErrorQueryHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IRplFundingCalculationService rplFundingCalculationService)
    : IRequestHandler<GetCohortPriorLearningErrorQuery, GetCohortPriorLearningErrorQueryResult>
{
    public async Task<GetCohortPriorLearningErrorQueryResult> Handle(GetCohortPriorLearningErrorQuery request, CancellationToken cancellationToken)
    {
        var draftApprenticeshipIds = new List<long>();

        var query = await dbContext.Value.DraftApprenticeships
            .Include(x => x.PriorLearning)
            .Where(x => x.CommitmentId == request.CohortId)
            .ToListAsync(cancellationToken);

        foreach (var draftApprenticeship in query)
        {
            var rplCalculation = await rplFundingCalculationService.GetRplFundingCalculations(
                draftApprenticeship.CourseCode,
                draftApprenticeship.StartDate,
                draftApprenticeship.PriorLearning?.DurationReducedByHours,
                draftApprenticeship.TrainingTotalHours,
                draftApprenticeship.PriorLearning?.PriceReducedBy,
                draftApprenticeship.PriorLearning?.IsDurationReducedByRpl,
                dbContext.Value.StandardFundingPeriods,
                dbContext.Value.FrameworkFundingPeriods
            );

            if (draftApprenticeship.PriorLearning != null && rplCalculation.RplPriceReductionError)
            {
                draftApprenticeshipIds.Add(draftApprenticeship.Id);
            }
        }

        return new GetCohortPriorLearningErrorQueryResult
        {
            DraftApprenticeshipIds = draftApprenticeshipIds,
        };
    }
}