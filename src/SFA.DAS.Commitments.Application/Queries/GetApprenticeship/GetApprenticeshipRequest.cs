using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeship
{
    public sealed class GetApprenticeshipRequest : IAsyncRequest<GetApprenticeshipResponse>
    {
        public Caller Caller { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
