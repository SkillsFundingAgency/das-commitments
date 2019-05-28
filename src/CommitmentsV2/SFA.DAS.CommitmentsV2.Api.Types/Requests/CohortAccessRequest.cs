using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CohortAccessRequest
    {
        public PartyType PartyType { get; set; }
        public string PartyId { get; set; }
        public long CohortId { get; set; }
    }
}