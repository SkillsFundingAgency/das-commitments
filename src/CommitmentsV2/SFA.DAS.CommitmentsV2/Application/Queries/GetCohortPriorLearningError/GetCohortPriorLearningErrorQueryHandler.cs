using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortPriorLearningError
{
    public class GetCohortPriorLearningErrorQueryHandler : IRequestHandler<GetCohortPriorLearningErrorQuery, GetCohortPriorLearningErrorQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IRplFundingCalculationService _rplFundingCalculationService;


        public GetCohortPriorLearningErrorQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IRplFundingCalculationService rplFundingCalculationService)
        {
            _dbContext = dbContext;
            _rplFundingCalculationService = rplFundingCalculationService;
        }

        public async Task<GetCohortPriorLearningErrorQueryResult> Handle(GetCohortPriorLearningErrorQuery request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipIds = new List<long>();

            var query = _dbContext.Value.DraftApprenticeships
                .Include(x => x.PriorLearning)
                .Where(x => x.CommitmentId == request.CohortId)
                .ToList();

            foreach (var draftApprenticeship in query)
            {

                var rplCalculation = await _rplFundingCalculationService.GetRplFundingCalculations(
                                                                    draftApprenticeship.CourseCode,
                                                                    draftApprenticeship.StartDate,
                                                                    draftApprenticeship.PriorLearning?.DurationReducedByHours,
                                                                    draftApprenticeship.TrainingTotalHours,
                                                                    draftApprenticeship.PriorLearning?.PriceReducedBy,
                                                                    draftApprenticeship.PriorLearning?.IsDurationReducedByRpl,
                                                                    _dbContext.Value.StandardFundingPeriods,
                                                                    _dbContext.Value.FrameworkFundingPeriods
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
}
