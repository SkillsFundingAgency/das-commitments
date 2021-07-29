using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class TriageDataLocksRequest : SaveDataRequest
    {
        public long ApprenticeshipId { get; set; }
        public TriageStatus TriageStatus { get; set; }
    }
}
