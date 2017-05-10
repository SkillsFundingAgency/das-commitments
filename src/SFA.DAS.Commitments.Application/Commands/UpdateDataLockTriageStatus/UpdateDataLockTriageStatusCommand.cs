using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus
{
    public sealed class UpdateDataLockTriageStatusCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public long DataLockEventId { get; set; }
        public Api.Types.DataLock.Types.TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }
    }
}
