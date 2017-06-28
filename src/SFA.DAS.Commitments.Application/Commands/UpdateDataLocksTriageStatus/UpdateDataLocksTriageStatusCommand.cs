using MediatR;

using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus
{
    public sealed class UpdateDataLocksTriageStatusCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }
    }
}
