using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Entities;
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

        public BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler(ILogger<BulkUploadAddDraftApprenticeshipCommandHandler> logger,
            IModelMapper draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService,
            IReservationsApiClient reservationsApiClient,
            IMediator mediator)
        {
            _logger = logger;
            _modelMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
            _reservationApiClient = reservationsApiClient;
            _mediator = mediator;
        }

        //CON-4187 Approve all and send to employers

        public async Task<BulkUploadAddAndApproveDraftApprenticeshipsResponse> Handle(BulkUploadAddAndApproveDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
        {
            //var command = new BulkUploadAddDraftApprenticeshipsCommand
            //{
            //    UserInfo = request.UserInfo,
            //    BulkUploadDraftApprenticeships = request.BulkUploadDraftApprenticeships,
            //    ProviderId = request.ProviderId
            //};

            var response = await _mediator.Send(new BulkUploadAddDraftApprenticeshipsCommand { UserInfo = request.UserInfo , BulkUploadDraftApprenticeships = request.BulkUploadDraftApprenticeships, ProviderId = request.ProviderId });

            //var handler = new BulkUploadAddDraftApprenticeshipCommandHandler(_logger, _modelMapper, _cohortDomainService, _reservationApiClient);
            //var response =  await handler.Handle(command, cancellationToken);
            
            foreach(var res in response.BulkUploadAddDraftApprenticeshipsResponse)
            {
                // Get CohortId by cohortreference
                //res.CohortReference

                await _cohortDomainService.ApproveCohort(123, "", request.UserInfo, cancellationToken);
            }

            var BulkUploadAddAndApproveDraftApprenticeshipsResponse = new BulkUploadAddAndApproveDraftApprenticeshipsResponse();

            return BulkUploadAddAndApproveDraftApprenticeshipsResponse;
        }
    }
}
