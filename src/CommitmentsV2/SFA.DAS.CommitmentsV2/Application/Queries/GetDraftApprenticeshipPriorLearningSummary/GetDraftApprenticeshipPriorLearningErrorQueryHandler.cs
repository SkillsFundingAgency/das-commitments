using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningError
{
    public class GetDraftApprenticeshipPriorLearningErrorQueryHandler : IRequestHandler<GetDraftApprenticeshipPriorLearningErrorQuery, GetDraftApprenticeshipPriorLearningErrorQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDraftApprenticeshipPriorLearningErrorQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDraftApprenticeshipPriorLearningErrorQueryResult> Handle(GetDraftApprenticeshipPriorLearningErrorQuery request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipIds = new List<long>();

            var query = _dbContext.Value.DraftApprenticeships
                .Include(x => x.PriorLearning)
                .Where(x => x.CommitmentId == request.CohortId);

            foreach (var draftApprenticeship in query)
            {
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

                if (HasRplPriceReductionError(x))
                {
                    draftApprenticeshipIds.Add(draftApprenticeship.Id);
                }
            }

            return new GetDraftApprenticeshipPriorLearningErrorQueryResult
            {
                DraftApprenticeshipIds = draftApprenticeshipIds,
            };
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
