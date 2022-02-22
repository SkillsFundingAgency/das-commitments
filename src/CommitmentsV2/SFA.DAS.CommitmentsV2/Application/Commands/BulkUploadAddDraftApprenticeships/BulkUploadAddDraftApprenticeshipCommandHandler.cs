using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipCommandHandler : AsyncRequestHandler<BulkUploadAddDraftApprenticeshipsCommand>
    {
        private readonly ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly IProviderCommitmentsDbContext _providerDbContext;
        private readonly Dictionary<long, long> _cohortIds;

        public BulkUploadAddDraftApprenticeshipCommandHandler(
            ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IProviderCommitmentsDbContext providerCommitmentsDbContext)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _providerDbContext = providerCommitmentsDbContext;
            _cohortIds = new Dictionary<long, long>();
        }

        protected override async Task Handle(BulkUploadAddDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeships = await _modelMapper.Map<List<DraftApprenticeshipDetails>>(request);
            foreach (var draftApprenticeship in draftApprenticeships)
            {
                var cohortId = await GetCohortId(request.BulkUploadDraftApprenticeships.First(x => x.Uln == draftApprenticeship.Uln), request.UserInfo, cancellationToken);
                var result = await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, cohortId, draftApprenticeship, request.UserInfo, cancellationToken);
                _logger.LogInformation($"Bulk upload - Added draft apprenticeship. Reservation-Id:{draftApprenticeship.ReservationId} Commitment-Id:{cohortId}");
            }
        }

        private async Task<long> GetCohortId(BulkUploadAddDraftApprenticeshipRequest bulkUploadAddDraftApprenticeshipRequest, UserInfo user, CancellationToken cancellation)
        {
            if (bulkUploadAddDraftApprenticeshipRequest.CohortId.HasValue)
            {
                if (_cohortIds.ContainsKey(bulkUploadAddDraftApprenticeshipRequest.LegalEntityId.Value))
                {
                    return _cohortIds.GetValueOrDefault(bulkUploadAddDraftApprenticeshipRequest.LegalEntityId.Value);
                }
                else
                {
                    var accountLegalEntity = _providerDbContext.AccountLegalEntities
                      .Include(x => x.Account)
                      .Where(x => x.Id == bulkUploadAddDraftApprenticeshipRequest.LegalEntityId).First();

                    var cohort = await _cohortDomainService.CreateEmptyCohort(bulkUploadAddDraftApprenticeshipRequest.ProviderId, accountLegalEntity.Account.Id, accountLegalEntity.Id, user, cancellation);
                    _cohortIds.Add(accountLegalEntity.Id, cohort.Id);
                }
            }

            return bulkUploadAddDraftApprenticeshipRequest.CohortId.Value;
        }
    }
}