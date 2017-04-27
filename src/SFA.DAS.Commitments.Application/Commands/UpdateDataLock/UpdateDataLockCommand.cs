using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLock
{
    public sealed class UpdateDataLockCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public long DataLockEventId { get; set; }
        public Api.Types.DataLock.DataLockStatus DataLockStatus { get; set; }
    }
}
