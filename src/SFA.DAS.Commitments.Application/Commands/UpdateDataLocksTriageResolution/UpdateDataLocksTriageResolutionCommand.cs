using MediatR;

using SFA.DAS.Commitments.Api.Types.DataLock.Types;

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
