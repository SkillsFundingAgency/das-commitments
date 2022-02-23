using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipCommandHandler : IRequestHandler<BulkUploadAddDraftApprenticeshipsCommand, GetBulkUploadAddDraftApprenticeshipsResponse>    
    {
        private readonly ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly IMediator _mediator;
        private readonly IProviderCommitmentsDbContext _providerDbContext;
        private readonly Dictionary<long, long> _cohortIds;

        public BulkUploadAddDraftApprenticeshipCommandHandler(
            ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IMediator mediator,
            IProviderCommitmentsDbContext providerCommitmentsDbContext)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _mediator = mediator;
            _providerDbContext = providerCommitmentsDbContext;
            _cohortIds = new Dictionary<long, long>();
        }

        public async Task<GetBulkUploadAddDraftApprenticeshipsResponse> Handle(BulkUploadAddDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            await Validate(request);

            var draftApprenticeshipsResponse = new List<BulkUploadAddDraftApprenticeshipsResponse>();
            var draftApprenticeships = await _modelMapper.Map<List<DraftApprenticeshipDetails>>(request);
            foreach (var draftApprenticeship in draftApprenticeships)
            {
                var cohortId = await GetCohortId(request.BulkUploadDraftApprenticeships.First(x => x.Uln == draftApprenticeship.Uln), request.UserInfo, cancellationToken);
                var result = await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, cohortId, draftApprenticeship, request.UserInfo, cancellationToken);

                _logger.LogInformation($"Bulk upload - Added draft apprenticeship. Reservation-Id:{draftApprenticeship.ReservationId} Commitment-Id:{cohortId}");
            }

            foreach (var cohortId in _cohortIds.Values)
            {
                var cohort = await _cohortDomainService.GetCohortDetails(cohortId, cancellationToken);
                draftApprenticeshipsResponse.Add(cohort);
            }

            return new GetBulkUploadAddDraftApprenticeshipsResponse { BulkUploadAddDraftApprenticeshipsResponse = draftApprenticeshipsResponse };
        }

        private async Task Validate(BulkUploadAddDraftApprenticeshipsCommand request)
        {
            await _mediator.Send(new BulkUploadValidateCommand { ProviderId = request.ProviderId, CsvRecords = request.BulkUploadDraftApprenticeships });
        }

        private async Task<long> GetCohortId(BulkUploadAddDraftApprenticeshipRequest bulkUploadAddDraftApprenticeshipRequest, UserInfo user, CancellationToken cancellation)
        {
            if (_cohortIds.ContainsKey(bulkUploadAddDraftApprenticeshipRequest.LegalEntityId.Value))
            {
                return _cohortIds.GetValueOrDefault(bulkUploadAddDraftApprenticeshipRequest.LegalEntityId.Value);
            }
            else
            {
                long cohortId;
                if (bulkUploadAddDraftApprenticeshipRequest.CohortId.HasValue)
                {
                    cohortId = bulkUploadAddDraftApprenticeshipRequest.CohortId.Value;
                }
                else
                {
                    var accountLegalEntity = _providerDbContext.AccountLegalEntities
                      .Include(x => x.Account)
                      .Where(x => x.Id == bulkUploadAddDraftApprenticeshipRequest.LegalEntityId).First();

                    var cohort = await _cohortDomainService.CreateEmptyCohort(bulkUploadAddDraftApprenticeshipRequest.ProviderId, accountLegalEntity.Account.Id, accountLegalEntity.Id, user, cancellation);

                    cohortId = cohort.Id;
                }
                _cohortIds.Add(bulkUploadAddDraftApprenticeshipRequest.LegalEntityId.Value, cohortId);
                return cohortId;
            }
        }

    }
}