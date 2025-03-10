using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ExpireInactiveCohortsWithEmployerAfter2Weeks;
public class ExpireInactiveCohortsWithEmployerAfter2WeeksHandler(
    Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
    ICurrentDateTime currentDateTime,
        CommitmentsV2Configuration configuration,

    ILogger<ExpireInactiveCohortsWithEmployerAfter2WeeksHandler> logger
    ) : IRequestHandler<ExpireInactiveCohortsWithEmployerAfter2WeeksCommand>
{
    public async Task Handle(ExpireInactiveCohortsWithEmployerAfter2WeeksCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var implementationDate = configuration.ExpireInactveEmployerCohortImplentationDate;
            var currentDate = currentDateTime.UtcNow;

            var recordsToExpire = await commitmentsDbContext.Value.Cohorts
                .Where(x => x.LastAction != LastAction.None
                            && x.IsDraft == false
                            && x.WithParty == Party.Employer
                            && x.LastUpdatedOn < currentDate.AddDays(-14).Date
                            && x.LastUpdatedOn > implementationDate)
                .ToListAsync(cancellationToken);

            foreach (var record in recordsToExpire)
            {
                record.SendToOtherParty(Party.Employer, "", UserInfo.System, currentDate);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling ExpireInactiveCohortsWithEmployerAfter2WeeksCommand");
            throw;
        }
    }
}
