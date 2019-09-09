using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsRequest : IRequest<GetDraftApprenticeshipsResult>
    {
        public GetDraftApprenticeshipsRequest(long cohortId)
        {
            CohortId = cohortId;
        }

        public long CohortId { get; set; }
    }
}
