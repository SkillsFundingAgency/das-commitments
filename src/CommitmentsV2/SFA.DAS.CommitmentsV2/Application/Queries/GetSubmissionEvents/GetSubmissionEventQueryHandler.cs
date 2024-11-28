using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents;

public class GetSubmissionEventQueryHandler(IApprovalsOuterApiClient approvalsOuterApiClient) : IRequestHandler<GetSubmissionEventsQuery, PageOfResults<SubmissionEvent>>
{
    public async Task<PageOfResults<SubmissionEvent>> Handle(GetSubmissionEventsQuery query, CancellationToken cancellationToken)
    {
        return await approvalsOuterApiClient.Get<PageOfResults<SubmissionEvent>>(new GetSubmissionsEventsRequest
        {
            SinceEventId = query.LastSubmissionEventId ?? 0,
        });
    }
}