using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class UpdateApprenticeshipStatusCommand : IAsyncRequest
    {
        public long AccountId { get; set; }
        public long CommitmentId { get; set; }
        public long ApprenticeshipId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public string UserId { get; set; }
    }
}
