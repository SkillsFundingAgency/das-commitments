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
        private readonly IRplFundingCalulationService _rplFundingCalulationService;


        public GetCohortPriorLearningErrorQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IRplFundingCalulationService rplFundingCalulationService)
        {
            _dbContext = dbContext;
            _rplFundingCalulationService = rplFundingCalulationService;
        }

        public async Task<GetCohortPriorLearningErrorQueryResult> Handle(GetCohortPriorLearningErrorQuery request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipIds = new List<long>();

            // testing
            draftApprenticeshipIds.Add(3);
            draftApprenticeshipIds.Add(4);
            draftApprenticeshipIds.Add(5);
            draftApprenticeshipIds.Add(6);
            draftApprenticeshipIds.Add(7);
            draftApprenticeshipIds.Add(8);
            // end testing

            var query = _dbContext.Value.DraftApprenticeships
                .Include(x => x.PriorLearning)
                .Where(x => x.CommitmentId == request.CohortId);

            foreach (var draftApprenticeship in query)
            {

                var rplCalculation = await _rplFundingCalulationService.GetRplFundingCalulations(
                                                                    draftApprenticeship.CourseCode,
                                                                    draftApprenticeship.StartDate,
                                                                    draftApprenticeship.PriorLearning.DurationReducedByHours,
                                                                    draftApprenticeship.TrainingTotalHours,
                                                                    draftApprenticeship.PriorLearning.PriceReducedBy,
                                                                    draftApprenticeship.PriorLearning.IsDurationReducedByRpl,
                                                                    _dbContext.Value.StandardFundingPeriods,
                                                                    _dbContext.Value.FrameworkFundingPeriods
                                                                    );

                if (rplCalculation.RplPriceReductionError)
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
