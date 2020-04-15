using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests
{
    public class GetChangeOfPartyRequestsQueryResult
    {
        public IReadOnlyCollection<ChangeOfPartyRequest> ChangeOfPartyRequests { get; set; }

        public class ChangeOfPartyRequest
        {
            public long Id { get; set; }
            public ChangeOfPartyRequestType ChangeOfPartyType { get; set; }
            public Party OriginatingParty { get; set; }
            public ChangeOfPartyRequestStatus Status { get; set; }
        }
    }
}
