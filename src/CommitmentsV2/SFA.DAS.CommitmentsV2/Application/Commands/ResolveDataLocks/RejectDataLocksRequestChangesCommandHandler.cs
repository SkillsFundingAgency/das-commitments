using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;

public class RejectDataLocksRequestChangesCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<RejectDataLocksRequestChangesCommandHandler> logger)
    : IRequestHandler<RejectDataLocksRequestChangesCommand>
{
    public async Task Handle(RejectDataLocksRequestChangesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Rejecting Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);

        var apprenticeship = await db.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

        var dataLocksToBeRejected = apprenticeship.DataLockStatus
            .Where(DataLockStatusExtensions.UnHandled)
            .Where(m => m.TriageStatus == TriageStatus.Change);

        if (apprenticeship.HasHadDataLockSuccess)
        {
            dataLocksToBeRejected = dataLocksToBeRejected.Where(DataLockStatusExtensions.IsPriceOnly);
        }

        if (!dataLocksToBeRejected.Any())
        {
            return;
        }

        apprenticeship.RejectDataLocks(Party.Employer, dataLocksToBeRejected.Select(m => m.DataLockEventId).ToList(), request.UserInfo);

        logger.LogInformation("Rejected Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
    }
}