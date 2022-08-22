using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest
{
    public class CreateOverlappingTrainingDateRequestCommandHandler : IRequestHandler<CreateOverlappingTrainingDateRequestCommand, CreateOverlappingTrainingDateResult>
    {
        private readonly IOverlappingTrainingDateRequestDomainService _overlappingTrainingDateRequestDomainService;

        public CreateOverlappingTrainingDateRequestCommandHandler(IOverlappingTrainingDateRequestDomainService overlappingTrainingDateRequestDomainService)
        {
            _overlappingTrainingDateRequestDomainService = overlappingTrainingDateRequestDomainService;
        }

        public async Task<CreateOverlappingTrainingDateResult> Handle(CreateOverlappingTrainingDateRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _overlappingTrainingDateRequestDomainService
                .CreateOverlappingTrainingDateRequest(request.DraftApprneticeshipId,
                request.UserInfo,
                cancellationToken);

            result.EmitOverlappingTrainingDateNotificationEvent(result.PreviousApprenticeshipId, result.DraftApprenticeship.Uln);

            return new CreateOverlappingTrainingDateResult
            {
                Id = result.Id
            };
        }
    }
}
