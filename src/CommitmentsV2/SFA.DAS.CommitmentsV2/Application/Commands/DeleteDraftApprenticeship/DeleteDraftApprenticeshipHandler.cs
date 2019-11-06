using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship
{
    public class DeleteDraftApprenticeshipHandler : IRequestHandler<DeleteDraftApprenticeshipCommand, DeleteDraftApprenticeshipResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<DeleteDraftApprenticeshipHandler> _logger;
        private readonly ICohortDomainService _cohortDomainService;

        public DeleteDraftApprenticeshipHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<DeleteDraftApprenticeshipHandler> logger,
            ICohortDomainService cohortDomainService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _cohortDomainService = cohortDomainService;
        }

        public async Task<DeleteDraftApprenticeshipResponse> Handle(DeleteDraftApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            await _cohortDomainService.DeleteDraftApprenticeship(command.CohortId, command.ApprenticeshipId, command.UserInfo, cancellationToken);

            _logger.LogInformation($"Deleted apprenticeShip. Apprenticeship-Id:{command.ApprenticeshipId} Commitment-Id:{command.CohortId}");

            var response = new DeleteDraftApprenticeshipResponse
            {
                Id = command.CohortId,
                ApprenticeshipId = command.ApprenticeshipId
            };

            return response;
        }
    }
}
