using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment
{
    public sealed class GetRelationshipByCommitmentRequest : IAsyncRequest<GetRelationshipByCommitmentResponse>
    {
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
    }
}
