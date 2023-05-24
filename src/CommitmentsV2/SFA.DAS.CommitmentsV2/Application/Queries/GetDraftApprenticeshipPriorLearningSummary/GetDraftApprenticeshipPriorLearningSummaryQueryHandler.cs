using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary
{
    public class GetDraftApprenticeshipPriorLearningSummaryQueryHandler : IRequestHandler<GetDraftApprenticeshipPriorLearningSummaryQuery, GetDraftApprenticeshipPriorLearningSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDraftApprenticeshipPriorLearningSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDraftApprenticeshipPriorLearningSummaryQueryResult> Handle(GetDraftApprenticeshipPriorLearningSummaryQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Value.DraftApprenticeships
                .Include(x => x.PriorLearning)
                .Where(x => x.Id == request.DraftApprenticeshipId && x.CommitmentId == request.CohortId);

            var x = await query.Select(draft => new GetDraftApprenticeshipPriorLearningSummaryQueryResult
            {
                CourseCode = draft.CourseCode,
                RecognisePriorLearning = draft.RecognisePriorLearning,
                TrainingTotalHours = draft.PriorLearning != null ? draft.TrainingTotalHours : null,
                DurationReducedByHours = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedByHours : null,
                IsDurationReducedByRpl = draft.PriorLearning != null ? draft.PriorLearning.IsDurationReducedByRpl : null,
                DurationReducedBy = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedBy : null,
                CostBeforeRpl = draft.PriorLearning != null ? draft.CostBeforeRpl : null,
                PriceReducedBy = draft.PriorLearning != null ? draft.PriorLearning.PriceReducedBy : null,
                StandardUId = draft.StandardUId,
                StartDate = draft.StartDate
            }).SingleOrDefaultAsync(cancellationToken);

            if (x != null && x.RecognisePriorLearning == true)
            {
                x.FundingBandMaximum = await GetFundingBandMaximum(x.CourseCode, x.StartDate);
                x.PercentageOfPriorLearning = CalculatePercentageOfPriorLearning(x.DurationReducedByHours, x.TrainingTotalHours);
                x.MinimumPercentageReduction = CalculateMinimumPercentageOfPriorLearning(x.PercentageOfPriorLearning);
                x.MinimumPriceReduction = CalculateMinimumPriceReduction(x.FundingBandMaximum, x.MinimumPercentageReduction);
                x.RplPriceReductionError = HasRplPriceReductionError(x);
                return x;
            }

            return null;
        }

        private async Task<int?> GetFundingBandMaximum(string courseCode, DateTime? startDate)
        {
            if (string.IsNullOrEmpty(courseCode))
                return null;

            if (int.TryParse(courseCode, out var standardId))
            {
                var standard = await _dbContext.Value.StandardFundingPeriods.Where(c => c.Id.Equals(standardId) 
                    && c.EffectiveFrom <= startDate && (c.EffectiveTo == null || c.EffectiveTo >= startDate))
                    .OrderByDescending(x=>x.EffectiveFrom).FirstOrDefaultAsync();
                return standard?.FundingCap;
            }

            var framework = await _dbContext.Value.FrameworkFundingPeriods.Where(c => c.Id.Equals(courseCode)
                    && c.EffectiveFrom <= startDate && (c.EffectiveTo == null || c.EffectiveTo >= startDate))
                .OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync();
            return framework?.FundingCap;
        }

        static decimal? CalculatePercentageOfPriorLearning(int? durationReducedByHours, int? trainingTotalHours)
        {
            if (durationReducedByHours == null || trainingTotalHours == null || trainingTotalHours == 0)
                return null;
            return (decimal)durationReducedByHours / trainingTotalHours * 100;
        }
        
        static decimal? CalculateMinimumPercentageOfPriorLearning(decimal? percentageOfPriorLearning)
        {
            if (percentageOfPriorLearning == null)
                return null;
            return percentageOfPriorLearning / 2;
        }

        static bool HasRplPriceReductionError(GetDraftApprenticeshipPriorLearningSummaryQueryResult x)
        {
            if (x.PriceReducedBy == null || x.MinimumPriceReduction == null)
                return false;
            return x.PriceReducedBy < x.MinimumPriceReduction;
        }

        static int? CalculateMinimumPriceReduction(int? fundingBandMaximum, decimal? minimumPercentageReduction)
        {
            if (fundingBandMaximum == null || minimumPercentageReduction == null || minimumPercentageReduction == 0)
                return null;
            return (int?)(fundingBandMaximum * minimumPercentageReduction / 100);
        }
    }
}
