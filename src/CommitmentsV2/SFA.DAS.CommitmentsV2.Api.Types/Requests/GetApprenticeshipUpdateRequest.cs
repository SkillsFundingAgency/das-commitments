using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class GetApprenticeshipUpdateRequest
    {
        public long ApprenticeshipId { get; set; }

        public ApprenticeshipUpdateStatus Status { get; set; }
    }
}
