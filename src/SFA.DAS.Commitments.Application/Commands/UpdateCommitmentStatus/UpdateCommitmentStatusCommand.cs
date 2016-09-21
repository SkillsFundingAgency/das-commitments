using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus
{
    public sealed class UpdateCommitmentStatusCommand : IAsyncRequest
    {
        public long AccountId { get; set; }
        public long CommitmentId { get; set; }
        public CommitmentStatus? Status { get; set; }
    }
}
