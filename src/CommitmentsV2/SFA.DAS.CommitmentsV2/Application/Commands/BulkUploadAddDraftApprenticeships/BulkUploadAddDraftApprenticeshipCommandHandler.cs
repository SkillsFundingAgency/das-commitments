using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipCommandHandler : IRequestHandler<BulkUploadAddDraftApprenticeshipsCommand, GetBulkUploadAddDraftApprenticeshipsResponse>
    {
        private readonly ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly ProviderCommitmentsDbContext _providerDbContext;
        private readonly IEncodingService _encodingService;
        private readonly IMediator _mediator;

        public BulkUploadAddDraftApprenticeshipCommandHandler(
            ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            ProviderCommitmentsDbContext providerCommitmentsDbContext,
            IEncodingService encodingService,
            IMediator mediator)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _providerDbContext = providerCommitmentsDbContext;
            _encodingService = encodingService;
            _mediator = mediator;
        }

        public async Task<GetBulkUploadAddDraftApprenticeshipsResponse> Handle(BulkUploadAddDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeships = await _modelMapper.Map<List<DraftApprenticeshipDetails>>(request);
            var cohorts = await _cohortDomainService.AddDraftApprenticeships(draftApprenticeships,
                request.BulkUploadDraftApprenticeships,
                request.ProviderId,
                request.UserInfo,
                cancellationToken);

            await _providerDbContext.SaveChangesAsync();

            await UpdateCohortReferences(cohorts);

            if (request.LogId != null)
            {
                await _cohortDomainService.RecordSaveActionForFileUpload(request.LogId.Value, "SaveAsDraft", cohorts, cancellationToken);
            }

            var cohortSummaryForBulkUpload = cohorts.Select(cohort => new BulkUploadAddDraftApprenticeshipsResponse
            {
                CohortReference = cohort.Reference,
                NumberOfApprenticeships = cohort.Apprenticeships.Count(),
                EmployerName = cohort.AccountLegalEntity.Name
            });

            return new GetBulkUploadAddDraftApprenticeshipsResponse { BulkUploadAddDraftApprenticeshipsResponse = cohortSummaryForBulkUpload };
        }

        private async Task UpdateCohortReferences(IEnumerable<Cohort> cohorts)
        {
            bool anyNewCohorts = false;
            foreach (var cohort in cohorts.Where(x => string.IsNullOrWhiteSpace(x.Reference)))
            {
                cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
                anyNewCohorts = true;
            }

            if (anyNewCohorts)
            {
                // Another save for cohort references
                await _providerDbContext.SaveChangesAsync();
            }
        }
    }
}