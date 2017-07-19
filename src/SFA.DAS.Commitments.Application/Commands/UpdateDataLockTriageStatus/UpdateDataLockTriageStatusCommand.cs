using MediatR;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus
{
    public sealed class UpdateDataLockTriageStatusCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public long DataLockEventId { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }
    }
}
