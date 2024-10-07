using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Authentication;

namespace SFA.DAS.CommitmentsV2.Application.Commands.TriageDataLocks;

public class TriageDataLocksCommandHandler(
    Lazy<ProviderCommitmentsDbContext> db,
    ILogger<TriageDataLocksCommandHandler> logger, 
    IAuthenticationService authenticationService)
    : IRequestHandler<TriageDataLocksCommand>
{
    public async Task Handle(TriageDataLocksCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Triage Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);

        var apprenticeship = await db.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

        var dataLocksToBeUpdated = apprenticeship.DataLockStatus
            .Where(DataLockStatusExtensions.UnHandled)
            .ToList();

        Validate(request, dataLocksToBeUpdated, apprenticeship);
            
        if (apprenticeship.HasHadDataLockSuccess && request.TriageStatus == TriageStatus.Change)
        {
            dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockStatusExtensions.IsPriceOnly).ToList();
        }

        if (dataLocksToBeUpdated.Exists(m => m.TriageStatus == request.TriageStatus))
        {                
            throw new InvalidOperationException($"Trying to update data lock for apprenticeship: {request.ApprenticeshipId} with the same TriageStatus ({request.TriageStatus}) ");
        }

        if (dataLocksToBeUpdated.Count != 0)
        {
            apprenticeship.TriageDataLocks(authenticationService.GetUserParty(), dataLocksToBeUpdated.Select(m => m.DataLockEventId).ToList(), request.TriageStatus, request.UserInfo);
        }

        logger.LogInformation("Triage Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
    }       

    private static void Validate(TriageDataLocksCommand command, List<DataLockStatus> dataLocksToBeUpdated, Apprenticeship apprenticeship)
    {
        var courseAndPriceOrOnlyCourse = dataLocksToBeUpdated.TrueForAll(DataLockStatusExtensions.WithCourseError)
                                         || dataLocksToBeUpdated.Exists(DataLockStatusExtensions.WithCourseAndPriceError);

        if (courseAndPriceOrOnlyCourse
            && apprenticeship.HasHadDataLockSuccess              
            && command.TriageStatus == TriageStatus.Change)
        {               
            throw new InvalidOperationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with triage status ({command.TriageStatus}) and datalock with course and price when Successful DataLock already received");
        }
    }       
}