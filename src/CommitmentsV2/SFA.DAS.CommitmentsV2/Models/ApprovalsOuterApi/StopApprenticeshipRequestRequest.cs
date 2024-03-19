using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi
{
    public class StopApprenticeshipRequestRequest : IPostApiRequest
    {
        public long ApprenticeshipId { get; }

        public StopApprenticeshipRequestRequest(long apprenticeshipId, Body body)
        {
            ApprenticeshipId = apprenticeshipId;
            Data = body;
        }

        public string PostUrl => $"{ApprenticeshipId}/stop";
        public object Data { get; set; }

        public class Body
        {
            public long AccountId { get; set; }
            public DateTime StopDate { get; set; }
            public bool MadeRedundant { get; set; }
            public UserInfo UserInfo { get; set; }
        }
    }
}