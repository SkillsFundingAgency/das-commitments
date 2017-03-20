using MediatR;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipCommand : IAsyncRequest
    {
        public Caller Caller { get; set; }
        public long CommitmentId { get; set; }
        public long ApprenticeshipId { get; set; }
        public Apprenticeship.Apprenticeship Apprenticeship { get; set; }

        public string UserId { get; set; }
    }
}
