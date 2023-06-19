using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary
{
    public class GetDraftApprenticeshipPriorLearningSummaryQueryHandler : IRequestHandler<GetDraftApprenticeshipPriorLearningSummaryQuery, GetDraftApprenticeshipPriorLearningSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IRplFundingCalulationService _rplFundingCalulationService;


        public GetDraftApprenticeshipPriorLearningSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IRplFundingCalulationService rplFundingCalulationService)
        {
            _dbContext = dbContext;
            _rplFundingCalulationService = rplFundingCalulationService;
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
                PriceReducedBy = draft.PriorLearning != null ? draft.PriorLearning.PriceReducedBy : null,
                StandardUId = draft.StandardUId,
                StartDate = draft.StartDate
            }).SingleOrDefaultAsync(cancellationToken);

            if (x != null && x.RecognisePriorLearning == true)
            {
                var rplCalculation = await _rplFundingCalulationService.GetRplFundingCalulations(
                                                                    x.CourseCode,
                                                                    x.StartDate,
                                                                    x.DurationReducedByHours,
                                                                    x.TrainingTotalHours,
                                                                    x.PriceReducedBy,
                                                                    x.IsDurationReducedByRpl,
                                                                    _dbContext.Value.StandardFundingPeriods,
                                                                    _dbContext.Value.FrameworkFundingPeriods
                                                                    );

                x.FundingBandMaximum = rplCalculation.FundingBandMaximum;
                x.PercentageOfPriorLearning = rplCalculation.PercentageOfPriorLearning;
                x.MinimumPercentageReduction = rplCalculation.MinimumPercentageReduction;
                x.MinimumPriceReduction = rplCalculation.MinimumPriceReduction;
                x.RplPriceReductionError = rplCalculation.RplPriceReductionError;
                return x;
            }

            return null;
        }
    }
}
