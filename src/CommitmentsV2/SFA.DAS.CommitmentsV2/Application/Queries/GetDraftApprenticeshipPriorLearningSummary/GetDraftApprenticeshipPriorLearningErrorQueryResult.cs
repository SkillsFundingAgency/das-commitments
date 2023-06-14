using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningError
{
    public class GetDraftApprenticeshipPriorLearningErrorQueryResult
    {
        public IEnumerable<long> DraftApprenticeshipIds { get; set; }
    }
}