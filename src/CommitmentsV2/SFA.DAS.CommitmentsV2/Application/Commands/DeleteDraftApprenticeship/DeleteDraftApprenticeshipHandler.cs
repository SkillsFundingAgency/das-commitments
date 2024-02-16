using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;

public class DeleteDraftApprenticeshipHandler : IRequestHandler<DeleteDraftApprenticeshipCommand>
{
    private readonly ILogger<DeleteDraftApprenticeshipHandler> _logger;
    private readonly ICohortDomainService _cohortDomainService;

    public DeleteDraftApprenticeshipHandler(ILogger<DeleteDraftApprenticeshipHandler> logger,
        ICohortDomainService cohortDomainService)
    {
        _logger = logger;
        _cohortDomainService = cohortDomainService;
    }

    public async Task Handle(DeleteDraftApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _cohortDomainService.DeleteDraftApprenticeship(command.CohortId, command.ApprenticeshipId, command.UserInfo, cancellationToken);

            _logger.LogInformation("Deleted apprenticeShip. Apprenticeship-Id:{ApprenticeshipId} Cohort-Id:{CohortId}", command.ApprenticeshipId, command.CohortId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error Deleting Apprenticeship");
            throw;
        }
    }
}