using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest
{
    public class ResolveOverlappingTrainingDateRequestCommandHandler : AsyncRequestHandler<ResolveOverlappingTrainingDateRequestCommand>
    {
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;

        public ResolveOverlappingTrainingDateRequestCommandHandler(IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        protected override async Task Handle(ResolveOverlappingTrainingDateRequestCommand request, CancellationToken cancellationToken)
        {
            await _resolveOverlappingTrainingDateRequestService.Resolve(request.ApprenticeshipId, request.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.ApprentieshipIsStillActive);
        }
    }
}