using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship
{
    public class UpdateDraftApprenticeshipHandler : IRequestHandler<UpdateDraftApprenticeshipCommand, UpdateDraftApprenticeshipResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<AddCohortHandler> _logger;
        private readonly IMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly ICohortDomainService _cohortDomainService;

        public UpdateDraftApprenticeshipHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<AddCohortHandler> logger,
            IMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
        }

        public async Task<UpdateDraftApprenticeshipResponse> Handle(UpdateDraftApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(command);

            await _cohortDomainService.UpdateDraftApprenticeship(command.CohortId, draftApprenticeshipDetails, command.UserInfo,  cancellationToken);

            _logger.LogInformation($"Saved cohort. Reservation-Id:{command.ReservationId} Commitment-Id:{command.CohortId} Apprenticeship:{command.ApprenticeshipId}");

            var response = new UpdateDraftApprenticeshipResponse
            {
                Id = command.CohortId,
                ApprenticeshipId = command.ApprenticeshipId
            };

            return response;
        }
    }
}
