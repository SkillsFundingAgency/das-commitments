using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort;

public class DeleteCohortHandler : IRequestHandler<DeleteCohortCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<DeleteCohortHandler> _logger;
    private readonly IAuthenticationService _authenticationService;

    public DeleteCohortHandler(
        Lazy<ProviderCommitmentsDbContext> dbContext,
        ILogger<DeleteCohortHandler> logger,
        IAuthenticationService authenticationService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _authenticationService = authenticationService;
    }

    public async Task Handle(DeleteCohortCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var db = _dbContext.Value;
            var cohort = await db.Cohorts.Include(c => c.Apprenticeships)
                .Include(c => c.Provider)
                .Include(c => c.AccountLegalEntity)
                .Include(c => c.TransferRequests)
                .SingleOrDefaultAsync(c => c.Id == command.CohortId, cancellationToken);

            cohort.Delete(_authenticationService.GetUserParty(), command.UserInfo);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cohort marked as deleted. Cohort-Id:{CohortId}", command.CohortId);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error Deleting Cohort");
            throw;
        }
    }
}