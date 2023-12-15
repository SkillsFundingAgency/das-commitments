using MediatR;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest
{
    public class CreateOverlappingTrainingDateRequestCommandHandler : IRequestHandler<
        CreateOverlappingTrainingDateRequestCommand, CreateOverlappingTrainingDateResult>
    {
        private readonly IOverlappingTrainingDateRequestDomainService _overlappingTrainingDateRequestDomainService;
        private readonly IAuthenticationService _authenticationService;

        public CreateOverlappingTrainingDateRequestCommandHandler(
            IOverlappingTrainingDateRequestDomainService overlappingTrainingDateRequestDomainService,
            IAuthenticationService authenticationService)
        {
            _overlappingTrainingDateRequestDomainService = overlappingTrainingDateRequestDomainService;
            _authenticationService = authenticationService;
        }

        public async Task<CreateOverlappingTrainingDateResult> Handle(
            CreateOverlappingTrainingDateRequestCommand request, CancellationToken cancellationToken)
        {
            var originatingParty = _authenticationService.GetUserParty();
            var result = await _overlappingTrainingDateRequestDomainService
                .CreateOverlappingTrainingDateRequest(request.DraftApprenticeshipId, originatingParty, null,
                    request.UserInfo,
                    cancellationToken);

            return new CreateOverlappingTrainingDateResult
            {
                Id = result.Id
            };
        }
    }
}