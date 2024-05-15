using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest
{
    public class GetOverlappingTrainingDateRequestQueryResult
    {
        public IList<OverlappingTrainingDateRequest> OverlappingTrainingDateRequests { get; set; } = new List<OverlappingTrainingDateRequest>();

        public class OverlappingTrainingDateRequest
        {
            public long Id { get; set; }
            public long DraftApprenticeshipId { get; set; }
            public long PreviousApprenticeshipId { get; set; }
            public OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
            public OverlappingTrainingDateRequestStatus Status { get; set; }
            public DateTime? ActionedOn { get; set; }
            public DateTime CreatedOn { get; set; }
        }
    }
}