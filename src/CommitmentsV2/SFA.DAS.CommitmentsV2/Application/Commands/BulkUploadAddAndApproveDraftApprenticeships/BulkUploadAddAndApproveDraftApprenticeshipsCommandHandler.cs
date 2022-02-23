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
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships
{
    public class BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler : IRequestHandler<BulkUploadAddAndApproveDraftApprenticeshipsCommand, BulkUploadAddAndApproveDraftApprenticeshipsResponse>
    {
        private readonly ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> _logger;
        private readonly IModelMapper _modelMapper;
        private readonly ICohortDomainService _cohortDomainService;
        private readonly IReservationsApiClient _reservationApiClient;
        private readonly IMediator _mediator;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler(ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IReservationsApiClient reservationsApiClient,
            IMediator mediator,
            Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _reservationApiClient = reservationsApiClient;
            _mediator = mediator;
            _dbContext = dbContext;
        }

        //CON-4187 Approve all and send to employers
        public async Task<BulkUploadAddAndApproveDraftApprenticeshipsResponse> Handle(BulkUploadAddAndApproveDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            var results = await _mediator.Send(new BulkUploadAddDraftApprenticeshipsCommand { UserInfo = request.UserInfo , BulkUploadDraftApprenticeships = request.BulkUploadDraftApprenticeships, ProviderId = request.ProviderId });
            
            foreach(var result in results.BulkUploadAddDraftApprenticeshipsResponse)
            {               
                var cohort = await _dbContext.Value.Cohorts.SingleAsync(c => c.Reference == result.CohortReference);
                await _cohortDomainService.ApproveCohort(cohort.Id, "", request.UserInfo, cancellationToken);
            }
            var response = new BulkUploadAddAndApproveDraftApprenticeshipsResponse()
            {
                BulkUploadAddAndApproveDraftApprenticeshipResponse = results.BulkUploadAddDraftApprenticeshipsResponse
            };

            return response;
        }       
    }
}
