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
            public string EmployerName { get; set; }
            public int? Price { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public long? CohortId { get; set; }
            public Party? WithParty { get; set; }
            public long? NewApprenticeshipId { get; set; }
            public long? ProviderId { get; set; }
        }
    }
}
