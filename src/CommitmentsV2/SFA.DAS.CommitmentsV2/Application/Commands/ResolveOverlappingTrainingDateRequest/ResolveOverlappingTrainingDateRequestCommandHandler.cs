using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest
{
    public class ResolveOverlappingTrainingDateRequestCommandHandler : IRequestHandler<ResolveOverlappingTrainingDateRequestCommand>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public ResolveOverlappingTrainingDateRequestCommandHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(ResolveOverlappingTrainingDateRequestCommand request, CancellationToken cancellationToken)
        {
            if (request?.ResolutionType == null)
                throw new ArgumentNullException(nameof(ResolveOverlappingTrainingDateRequestCommand.ResolutionType));

            await _resolveOverlappingTrainingDateRequestService.Resolve(request.ApprenticeshipId, null, request.ResolutionType.Value);
        }
    }
}