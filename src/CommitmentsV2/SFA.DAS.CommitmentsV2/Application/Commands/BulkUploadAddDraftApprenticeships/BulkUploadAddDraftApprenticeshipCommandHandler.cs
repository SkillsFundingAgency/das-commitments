﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipCommandHandler : AsyncRequestHandler<BulkUploadAddDraftApprenticeshipsCommand>
    {
        private readonly ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly IReservationsApiClient _reservationApiClient;

        public BulkUploadAddDraftApprenticeshipCommandHandler(
            ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IReservationsApiClient reservationsApiClient)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _reservationApiClient = reservationsApiClient;
        }

        protected override async Task Handle(BulkUploadAddDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeships = await _modelMapper.Map<List<DraftApprenticeshipDetails>>(request);
            foreach (var draftApprenticeship in draftApprenticeships)
            {
                var cohortId = request.BulkUploadDraftApprenticeships.First(x => x.Uln == draftApprenticeship.Uln).CohortId;
                var result = await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, cohortId, draftApprenticeship, request.UserInfo, cancellationToken);
                _logger.LogInformation($"Bulk upload - Added draft apprenticeship. Reservation-Id:{draftApprenticeship.ReservationId} Commitment-Id:{cohortId}");
            }
        }
    }
}