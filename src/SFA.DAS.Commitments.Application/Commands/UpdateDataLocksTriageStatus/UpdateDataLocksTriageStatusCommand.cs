using MediatR;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus
{
    public sealed class UpdateDataLocksTriageStatusCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }
    }
}
