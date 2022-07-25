using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails
{
    public class GetOverlappingApprenticeshipDetailsQueryResult
    {
        public long ApprenticeshipId { get; set; }
        public ApprenticeshipStatus Status { get; set; }
    }
}
