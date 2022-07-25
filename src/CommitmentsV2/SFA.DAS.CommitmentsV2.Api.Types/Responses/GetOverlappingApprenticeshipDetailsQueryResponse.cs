using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetOverlappingApprenticeshipDetailsResponse
    {
        public long ApprenticeshipId { get; set; }
        public ApprenticeshipStatus Status { get; set; }
    }
}
