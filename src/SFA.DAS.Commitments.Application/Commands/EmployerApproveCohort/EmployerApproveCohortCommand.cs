using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort
{
    public sealed class EmployerApproveCohortCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }
        public string LastUpdatedByEmail { get; set; }
        public string LastUpdatedByName { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
