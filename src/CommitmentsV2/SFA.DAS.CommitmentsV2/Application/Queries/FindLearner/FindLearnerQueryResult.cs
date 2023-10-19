using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.FindLearner
{
    public class FindLearnerQueryResult
    {
        public List<Learner> Learners { get; }
        public FindLearnerQueryResult(List<Learner> learners)
        {
            Learners = learners;
        }
    }
}
