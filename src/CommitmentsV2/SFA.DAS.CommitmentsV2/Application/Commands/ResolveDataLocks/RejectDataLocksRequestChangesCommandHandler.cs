using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;

public class RejectDataLocksRequestChangesCommandHandler : IRequestHandler<RejectDataLocksRequestChangesCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly ILogger<RejectDataLocksRequestChangesCommandHandler> _logger;

    public RejectDataLocksRequestChangesCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<RejectDataLocksRequestChangesCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(RejectDataLocksRequestChangesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);

        var apprenticeship = await _db.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

        var dataLocksToBeRejected = apprenticeship.DataLockStatus
            .Where(DataLockStatusExtensions.UnHandled)
            .Where(m => m.TriageStatus == TriageStatus.Change);

        if (apprenticeship.HasHadDataLockSuccess)
        {
            dataLocksToBeRejected = dataLocksToBeRejected.Where(DataLockStatusExtensions.IsPriceOnly);
        }

        if (!dataLocksToBeRejected.Any())
            return;

        apprenticeship.RejectDataLocks(Party.Employer, dataLocksToBeRejected.Select(m => m.DataLockEventId).ToList(), request.UserInfo);

        _logger.LogInformation("Rejected Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
    }
}