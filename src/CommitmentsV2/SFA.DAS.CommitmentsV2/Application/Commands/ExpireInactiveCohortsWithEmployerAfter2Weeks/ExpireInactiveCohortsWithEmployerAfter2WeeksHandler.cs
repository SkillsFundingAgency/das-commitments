using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ExpireInactiveCohortsWithEmployerAfter2Weeks;
public class ExpireInactiveCohortsWithEmployerAfter2WeeksHandler(
    ILogger<ExpireInactiveCohortsWithEmployerAfter2WeeksHandler> logger,
    IMessageSession messageSession,
    Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
    ICurrentDateTime currentDateTime,
    CommitmentsV2Configuration configuration
    ) : IRequestHandler<ExpireInactiveCohortsWithEmployerAfter2WeeksCommand>
{
    private const int ExpirationDays = 14;

    public async Task Handle(ExpireInactiveCohortsWithEmployerAfter2WeeksCommand command, CancellationToken cancellationToken)
    {
        var implementationDate = configuration.ExpireInactiveEmployerCohortImplementationDate;
        var currentDate = currentDateTime.UtcNow;

        var recordsToExpire = await GetCohortsToExpire(implementationDate, currentDate, cancellationToken);

        logger.LogInformation("ExpireInactiveCohortsWithEmployerAfter2WeeksCommand found {count} cohorts to expire", recordsToExpire.Count);
        foreach (var record in recordsToExpire)
        {
            logger.LogInformation("ExpireInactiveCohortsWithEmployerAfter2WeeksCommand sending {command} for cohort {id}", nameof(EmployerSendCohortCommand), record.Id);

            await messageSession.Send(new EmployerSendCohortCommand
            {
                CohortId = record.Id,
                Message = "",
                UserInfo = UserInfo.System
            });
        }
    }

    private async Task<List<Models.Cohort>> GetCohortsToExpire(DateTime implementationDate, DateTime currentDate, CancellationToken cancellationToken)
    {
        var expirationThreshold = currentDate.AddDays(-ExpirationDays).Date;

        return await commitmentsDbContext.Value.Cohorts
        .Where(cohort => cohort.LastAction != LastAction.None
            && !cohort.IsDraft
            && cohort.WithParty == Party.Employer
            && cohort.LastUpdatedOn < expirationThreshold
            && cohort.LastUpdatedOn > implementationDate)
        .ToListAsync(cancellationToken);
    }
}
