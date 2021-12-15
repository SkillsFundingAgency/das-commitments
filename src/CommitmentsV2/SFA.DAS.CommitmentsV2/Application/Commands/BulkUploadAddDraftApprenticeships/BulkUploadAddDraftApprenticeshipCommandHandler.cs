using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipCommandHandler : AsyncRequestHandler<BulkUploadAddDraftApprenticeshipsCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly IReservationsApiClient _reservationApiClient;

        public BulkUploadAddDraftApprenticeshipCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IReservationsApiClient reservationsApiClient)
        {
            _dbContext = dbContext;
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _reservationApiClient = reservationsApiClient;
        }

        protected override async Task Handle(BulkUploadAddDraftApprenticeshipsCommand requests, CancellationToken cancellationToken)
        {
            await MapReservation(requests, cancellationToken);

            var db = _dbContext.Value;
            foreach (var request in requests.DraftApprenticeships)
            {
                var draftApprenticeshipDetails = await _modelMapper.Map<DraftApprenticeshipDetails>(request);
                var draftApprenticeship = await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, request.CohortId, draftApprenticeshipDetails, requests.UserInfo, cancellationToken);

                _logger.LogInformation($"Bulk upload - Added draft apprenticeship. Reservation-Id:{request.ReservationId} Commitment-Id:{request.CohortId} Apprenticeship-Id:{draftApprenticeship.Id}");

            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task MapReservation(BulkUploadAddDraftApprenticeshipsCommand requests, CancellationToken cancellationToken)
        {
            var legalEntities = requests.DraftApprenticeships.GroupBy(x => x.LegalEntityId).Select(y => new { Id = y.Key, NumberOfApprentices = y.Count() });
            foreach (var legalEntity in legalEntities)
            {
                 var draftApprenticeshipForThisLegalEntity = requests.DraftApprenticeships.Where(x => x.LegalEntityId == legalEntity.Id).ToList();
                 var reservationIds = await _reservationApiClient.BulkCreateReservations(legalEntity.Id, new BulkCreateReservationsRequest { Count = ushort.Parse(legalEntity.NumberOfApprentices.ToString()) }, cancellationToken);

                for (int counter = 0; counter < legalEntity.NumberOfApprentices; counter++)
                {
                    draftApprenticeshipForThisLegalEntity[counter].ReservationId = reservationIds.ReservationIds[counter];
                }
                //reservationIds.ReservationIds.Zip<Guid, BulkUploadAddDraftApprenticeshipRequest>(draftApprenticeshipForThisLegalEntity, (reservationId, d) =>
                //{
                //    d.ReservationId = reservationId;
                // });
                //await Task.FromResult(0);
                //var reservationIds = new List<Guid>();
                //for(var i =0; i< legalEntity.NumberOfApprentices; i++)
                //{
                //    reservationIds.Add(Guid.NewGuid());
                //}
                //reservationIds.Zip(draftApprenticeshipForThisLegalEntity, (reservationId, d) => d.ReservationId = reservationId);
            }
        }
    }
}