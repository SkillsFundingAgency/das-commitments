using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Linq;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships
{
    public class BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler : IRequestHandler<BulkUploadAddAndApproveDraftApprenticeshipsCommand, BulkUploadAddAndApproveDraftApprenticeshipsResponse>
    {
        private readonly ILogger<BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly IMediator _mediator;
        private readonly IProviderCommitmentsDbContext _providerDbContext;

        public BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler(
            ILogger<BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IMediator mediator,
             IProviderCommitmentsDbContext providerDbContext)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _mediator = mediator;
            _providerDbContext = providerDbContext;
        }
        
        public async Task<BulkUploadAddAndApproveDraftApprenticeshipsResponse> Handle(BulkUploadAddAndApproveDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            var results = await _mediator.Send(new BulkUploadAddDraftApprenticeshipsCommand 
            { UserInfo = request.UserInfo, BulkUploadDraftApprenticeships = request.BulkUploadDraftApprenticeships, ProviderId = request.ProviderId });

            foreach (var result in results.BulkUploadAddDraftApprenticeshipsResponse)
            {
                var cohort = await _providerDbContext.Cohorts.SingleAsync(c => c.Reference == result.CohortReference);
                await _cohortDomainService.ApproveCohort(cohort.Id, "", request.UserInfo, Party.Provider, cancellationToken);
                _logger.LogInformation($"Bulk upload - Added and Approved  draft apprenticeship. Commitment-Reference:{cohort.Reference} Commitment-Id:{cohort.Id}");
            }

            return new BulkUploadAddAndApproveDraftApprenticeshipsResponse()
            {
                BulkUploadAddAndApproveDraftApprenticeshipResponse = results.BulkUploadAddDraftApprenticeshipsResponse
            };
        }
    }
}
