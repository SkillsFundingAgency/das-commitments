using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents
{
    public class GetSubmissionEventsQuery : IRequest<PageOfResults<SubmissionEvent>>
    {
        public long? LastSubmissionEventId { get; set; }
        public GetSubmissionEventsQuery(long? lastSubmissionEventId)
        {
            LastSubmissionEventId = lastSubmissionEventId;
        }
    }
}
