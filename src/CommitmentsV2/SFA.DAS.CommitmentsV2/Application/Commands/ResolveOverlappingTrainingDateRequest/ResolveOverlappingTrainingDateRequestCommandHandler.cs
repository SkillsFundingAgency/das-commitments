﻿using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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