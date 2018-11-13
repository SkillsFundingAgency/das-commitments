using SFA.DAS.Commitments.Api.Types.Commitment.Types;

namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    public sealed class CommitmentRequest
    {
        public string UserId { get; set; }

        public Commitment Commitment { get; set; }

        public string Message { get; set; }
        public LastAction LastAction { get; set; }
    }
}
