using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class ResolveApprenticeshipOverlappingTrainingDateRequest
    {
        public long? ApprenticeshipId { get; set; }
        public OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
    }
}