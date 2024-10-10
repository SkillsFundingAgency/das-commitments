using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;

public class AcceptDataLocksRequestChangesCommandHandler(
    Lazy<ProviderCommitmentsDbContext> db,
    ICurrentDateTime currentDateTime,
    ITrainingProgrammeLookup trainingProgrammeLookup,
    ILogger<AcceptDataLocksRequestChangesCommandHandler> logger)
    : IRequestHandler<AcceptDataLocksRequestChangesCommand>
{
    public async Task Handle(AcceptDataLocksRequestChangesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Accepting Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);

        var apprenticeship = await db.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

        IReadOnlyList<PriceHistory> currentPriceHistory = new List<PriceHistory>(apprenticeship.PriceHistory);
        var dataLocksToBeAccepted = apprenticeship.DataLockStatus
            .Where(DataLockStatusExtensions.UnHandled)
            .Where(m => m.TriageStatus == TriageStatus.Change);

        if (apprenticeship.HasHadDataLockSuccess)
        {
            dataLocksToBeAccepted = dataLocksToBeAccepted.Where(DataLockStatusExtensions.IsPriceOnly);
        }

        if (!dataLocksToBeAccepted.Any())
        {
            return;
        }

        var dataLockPasses = apprenticeship.DataLockStatus.Where(x => x.Status == Status.Pass || x.PreviousResolvedPriceDataLocks());

        var updatedPriceHistory = apprenticeship.CreatePriceHistory(dataLocksToBeAccepted, dataLockPasses);
        ReplacePriceHistory(apprenticeship, updatedPriceHistory, request.UserInfo, currentPriceHistory.ToList());

        if (!apprenticeship.HasHadDataLockSuccess)
        {
            var dataLockWithUpdatedTraining = dataLocksToBeAccepted.FirstOrDefault(m => m.IlrTrainingCourseCode != apprenticeship.CourseCode);
            if (dataLockWithUpdatedTraining != null)
            {
                var training = await trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(dataLockWithUpdatedTraining.IlrTrainingCourseCode, apprenticeship.StartDate.GetValueOrDefault());
                // PA-599 This is a temp fix, which will allow frameworks to be accepted
                if (training == null)
                {
                    training = await trainingProgrammeLookup.GetTrainingProgramme(dataLockWithUpdatedTraining.IlrTrainingCourseCode);
                }

                if (training != null)
                {
                    logger.LogInformation("Updating course for apprenticeship {ApprenticeshipId} from training code {CourseCode} to {IlrTrainingCourseCode}", apprenticeship.Id, apprenticeship.CourseCode, dataLockWithUpdatedTraining.IlrTrainingCourseCode);
                    apprenticeship.UpdateCourse(Party.Employer, dataLockWithUpdatedTraining.IlrTrainingCourseCode, training.Name, training.ProgrammeType, request.UserInfo, training.StandardUId, training.Version, currentDateTime.UtcNow);
                }
            }
        }

        apprenticeship.AcceptDataLocks(Party.Employer, currentDateTime.UtcNow, dataLocksToBeAccepted.Select(m => m.DataLockEventId).ToList(), request.UserInfo);

        logger.LogInformation("Accepted Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
    }

    private void ReplacePriceHistory(Apprenticeship apprenticeship, List<PriceHistory> updatedPriceHistory, UserInfo userInfo, List<PriceHistory> currentPriceHistory)
    {
        // price history entries are only identified for a given apprenticeship by their from date and to date; 
        // therefore it would be difficult to replace only the ones that have changed when there can be duplicate
        // entries with the same from date and to date
        foreach (var item in apprenticeship.PriceHistory)
        {
            db.Value.PriceHistory.Remove(item);
        }

        foreach (var item in updatedPriceHistory)
        {
            db.Value.PriceHistory.Add(item);
        }

        apprenticeship.ReplacePriceHistory(Party.Employer, currentPriceHistory, updatedPriceHistory, userInfo);
    }
}