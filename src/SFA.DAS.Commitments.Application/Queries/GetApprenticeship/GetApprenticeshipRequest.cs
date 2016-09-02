using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeship
{
    public sealed class GetApprenticeshipRequest : IAsyncRequest<GetApprenticeshipResponse>
    {
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long CommitmentId { get; set; }
    }
}
