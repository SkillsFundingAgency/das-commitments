using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort;

public class DeleteCohortHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DeleteCohortHandler> logger,
    IAuthenticationService authenticationService)
    : IRequestHandler<DeleteCohortCommand>
{
    public async Task Handle(DeleteCohortCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var db = dbContext.Value;
            var cohort = await db.Cohorts.Include(c => c.Apprenticeships)
                .Include(c => c.Provider)
                .Include(c => c.AccountLegalEntity)
                .Include(c => c.TransferRequests)
                .SingleOrDefaultAsync(c => c.Id == command.CohortId, cancellationToken);

            cohort.Delete(authenticationService.GetUserParty(), command.UserInfo);

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Cohort marked as deleted. Cohort-Id:{CohortId}", command.CohortId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Deleting Cohort");
            throw;
        }
    }
}