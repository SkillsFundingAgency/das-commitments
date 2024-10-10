using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;

public class GetDraftApprenticeshipPriorLearningSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IRplFundingCalculationService rplFundingCalculationService)
    : IRequestHandler<GetDraftApprenticeshipPriorLearningSummaryQuery, GetDraftApprenticeshipPriorLearningSummaryQueryResult>
{
    public async Task<GetDraftApprenticeshipPriorLearningSummaryQueryResult> Handle(GetDraftApprenticeshipPriorLearningSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Value.DraftApprenticeships
            .Include(x => x.PriorLearning)
            .Where(x => x.Id == request.DraftApprenticeshipId && x.CommitmentId == request.CohortId);

        var result = await query.Select(draft => new GetDraftApprenticeshipPriorLearningSummaryQueryResult
        {
            CourseCode = draft.CourseCode,
            RecognisePriorLearning = draft.RecognisePriorLearning,
            TrainingTotalHours = draft.PriorLearning != null ? draft.TrainingTotalHours : null,
            DurationReducedByHours = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedByHours : null,
            IsDurationReducedByRpl = draft.PriorLearning != null ? draft.PriorLearning.IsDurationReducedByRpl : null,
            DurationReducedBy = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedBy : null,
            PriceReducedBy = draft.PriorLearning != null ? draft.PriorLearning.PriceReducedBy : null,
            StandardUId = draft.StandardUId,
            StartDate = draft.StartDate
        }).SingleOrDefaultAsync(cancellationToken);

        if (result != null && result.RecognisePriorLearning == true)
        {
            var rplCalculation = await rplFundingCalculationService.GetRplFundingCalculations(
                result.CourseCode,
                result.StartDate,
                result.DurationReducedByHours,
                result.TrainingTotalHours,
                result.PriceReducedBy,
                result.IsDurationReducedByRpl,
                dbContext.Value.StandardFundingPeriods,
                dbContext.Value.FrameworkFundingPeriods
            );

            result.FundingBandMaximum = rplCalculation.FundingBandMaximum;
            result.PercentageOfPriorLearning = rplCalculation.PercentageOfPriorLearning;
            result.MinimumPercentageReduction = rplCalculation.MinimumPercentageReduction;
            result.MinimumPriceReduction = rplCalculation.MinimumPriceReduction;
            result.RplPriceReductionError = rplCalculation.RplPriceReductionError;
            
            return result;
        }

        return null;
    }
}