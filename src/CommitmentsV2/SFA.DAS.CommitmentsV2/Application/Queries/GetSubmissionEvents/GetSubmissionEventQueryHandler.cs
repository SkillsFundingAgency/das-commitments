using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents
{
    public class GetSubmissionEventQueryHandler : IRequestHandler<GetSubmissionEventsQuery, PageOfResults<SubmissionEvent>>
    {
        private readonly IApprovalsOuterApiClient _approvalOuterApiClient;

        public GetSubmissionEventQueryHandler(IApprovalsOuterApiClient approvalsOuterApiClient)
        {
            _approvalOuterApiClient = approvalsOuterApiClient;
        }

        public async Task<PageOfResults<SubmissionEvent>> Handle(GetSubmissionEventsQuery query, CancellationToken cancellationToken)
        {
            var result = await _approvalOuterApiClient.Get<PageOfResults<SubmissionEvent>>(new GetSubmissionsEventsRequest
            {
                SinceEventId = query.LastSubmissionEventId ?? 0,
            });

            return result;
        }
    }
}
