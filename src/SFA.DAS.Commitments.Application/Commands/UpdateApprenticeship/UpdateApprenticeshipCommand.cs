using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }
        public long ApprenticeshipId { get; set; }
        public Apprenticeship Apprenticeship { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
