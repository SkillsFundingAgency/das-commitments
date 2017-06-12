using MediatR;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageResolution
{
    public class UpdateDataLocksTriageResolutionCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public DataLockUpdateType DataLockUpdateType { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }

    }
}
