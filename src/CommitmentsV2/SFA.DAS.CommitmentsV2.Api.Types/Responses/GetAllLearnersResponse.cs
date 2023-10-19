using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetAllLearnersResponse
    {
        public int BatchNumber { get; set; }
        public int BatchSize { get; set; }
        public int TotalNumberOfBatches { get; set; }
        public List<Learner> Learners { get; set; }
    }
}
