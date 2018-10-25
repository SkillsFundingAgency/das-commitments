using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetApprovedApprenticeship
{
    public class GetApprovedApprenticeshipRequest: IAsyncRequest<GetApprovedApprenticeshipResponse>
    {
        public Caller Caller { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
