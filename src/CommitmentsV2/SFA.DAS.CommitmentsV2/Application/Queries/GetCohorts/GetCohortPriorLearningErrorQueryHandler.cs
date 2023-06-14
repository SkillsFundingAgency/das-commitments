using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Data;
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

        public GetCohortPriorLearningErrorQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetCohortPriorLearningErrorQueryResult> Handle(GetCohortPriorLearningErrorQuery request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipIds = new List<long>();

            var query =  _dbContext.Value.DraftApprenticeships
                .Include(x => x.PriorLearning)
                .Where(x => x.CommitmentId == request.CohortId);

            foreach (var draftApprenticeship in query)
            {
                GetDraftApprenticeshipPriorLearningSummaryQueryResult x = new GetDraftApprenticeshipPriorLearningSummaryQueryResult
                {
                    CourseCode = draftApprenticeship.CourseCode,
                    RecognisePriorLearning = draftApprenticeship.RecognisePriorLearning,
                    TrainingTotalHours = draftApprenticeship.PriorLearning != null ? draftApprenticeship.TrainingTotalHours : null,
                    DurationReducedByHours = draftApprenticeship.PriorLearning != null ? draftApprenticeship.PriorLearning.DurationReducedByHours : null,
                    IsDurationReducedByRpl = draftApprenticeship.PriorLearning != null ? draftApprenticeship.PriorLearning.IsDurationReducedByRpl : null,
                    DurationReducedBy = draftApprenticeship.PriorLearning != null ? draftApprenticeship.PriorLearning.DurationReducedBy : null,
                    PriceReducedBy = draftApprenticeship.PriorLearning != null ? draftApprenticeship.PriorLearning.PriceReducedBy : null,
                    StandardUId = draftApprenticeship.StandardUId,
                    StartDate = draftApprenticeship.StartDate
                };

                if (HasRplPriceReductionError(x))
                {
                    draftApprenticeshipIds.Add(draftApprenticeship.Id);
                }
            }

            return Task.FromResult(new GetCohortPriorLearningErrorQueryResult
            {
                DraftApprenticeshipIds = draftApprenticeshipIds,
            });
        }

        static bool HasRplPriceReductionError(GetDraftApprenticeshipPriorLearningSummaryQueryResult x)
        {
            if(!AreRplFieldsAreComplete(x))
                return false;
            return x.PriceReducedBy < x.MinimumPriceReduction;
        }

        static bool AreRplFieldsAreComplete(GetDraftApprenticeshipPriorLearningSummaryQueryResult x)
        {
            var areSet = x.TrainingTotalHours.HasValue && x.DurationReducedByHours.HasValue && x.PriceReducedBy.HasValue && x.IsDurationReducedByRpl.HasValue;
            if (!areSet) return false;
            switch (x.IsDurationReducedByRpl)
            {
                case true when !x.DurationReducedBy.HasValue:
                case false when x.DurationReducedBy.HasValue:
                    return false;
                default:
                    return true;
            }
        }
    }
}
