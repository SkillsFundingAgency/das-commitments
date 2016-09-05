using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    // Note: Have currently broken the CQRS pattern here as need to return the Id.
    public sealed class CreateApprenticeshipCommand : IAsyncRequest<long>
    {
        public Apprenticeship Apprenticeship { get; set; }

        public long CommitmentId { get; set; }
    }
}
