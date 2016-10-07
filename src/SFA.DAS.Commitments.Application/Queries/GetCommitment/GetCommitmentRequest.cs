using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitment
{
    public sealed class GetCommitmentRequest : IAsyncRequest<GetCommitmentResponse>
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }
    }
}
