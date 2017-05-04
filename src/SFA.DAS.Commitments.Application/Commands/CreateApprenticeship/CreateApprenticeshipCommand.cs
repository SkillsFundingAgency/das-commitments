using MediatR;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    // Note: Have currently broken the CQRS pattern here as need to return the Id.
    public sealed class CreateApprenticeshipCommand : IAsyncRequest<long>
    {
        public Caller Caller { get; set; }

        public long CommitmentId { get; set; }

        public Apprenticeship.Apprenticeship Apprenticeship { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
