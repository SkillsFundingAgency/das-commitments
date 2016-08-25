using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitment
{
    public sealed class GetCommitmentRequest : IAsyncRequest<GetCommitmentResponse>
    {
        public long CommitmentId { get; set; }

        public long? AccountId { get; set; }

        public long? ProviderId { get; set; }
    }
}
