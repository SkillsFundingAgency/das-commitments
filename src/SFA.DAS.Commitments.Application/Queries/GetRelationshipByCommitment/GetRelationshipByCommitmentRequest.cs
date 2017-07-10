using MediatR;

using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment
{
    public sealed class GetRelationshipByCommitmentRequest : IAsyncRequest<GetRelationshipByCommitmentResponse>
    {
        public Caller Caller { get; set; }
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }

    }
}
