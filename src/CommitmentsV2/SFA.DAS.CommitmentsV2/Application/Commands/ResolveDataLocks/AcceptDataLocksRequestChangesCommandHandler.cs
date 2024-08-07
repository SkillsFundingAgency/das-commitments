using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;

public class AcceptDataLocksRequestChangesCommandHandler : IRequestHandler<AcceptDataLocksRequestChangesCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;
    private readonly ILogger<AcceptDataLocksRequestChangesCommandHandler> _logger;

    public AcceptDataLocksRequestChangesCommandHandler(Lazy<ProviderCommitmentsDbContext> db,
        ICurrentDateTime currentDateTime,
        ITrainingProgrammeLookup trainingProgrammeLookup,
        ILogger<AcceptDataLocksRequestChangesCommandHandler> logger)
    {
        _db = db;
        _currentDateTime = currentDateTime;
        _trainingProgrammeLookup = trainingProgrammeLookup;
        _logger = logger;
    }

    public async Task Handle(AcceptDataLocksRequestChangesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Accepting Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);

        var apprenticeship = await _db.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

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
                var training = await _trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(dataLockWithUpdatedTraining.IlrTrainingCourseCode, apprenticeship.StartDate.GetValueOrDefault());
                // PA-599 This is a temp fix, which will allow frameworks to be accepted
                if (training == null)
                {
                    training = await _trainingProgrammeLookup.GetTrainingProgramme(dataLockWithUpdatedTraining.IlrTrainingCourseCode);
                }

                if (training != null)
                {
                    _logger.LogInformation("Updating course for apprenticeship {ApprenticeshipId} from training code {CourseCode} to {IlrTrainingCourseCode}", apprenticeship.Id, apprenticeship.CourseCode, dataLockWithUpdatedTraining.IlrTrainingCourseCode);
                    apprenticeship.UpdateCourse(Party.Employer, dataLockWithUpdatedTraining.IlrTrainingCourseCode, training.Name, training.ProgrammeType, request.UserInfo, training.StandardUId, training.Version, _currentDateTime.UtcNow);
                }
            }
        }

        apprenticeship.AcceptDataLocks(Party.Employer, _currentDateTime.UtcNow, dataLocksToBeAccepted.Select(m => m.DataLockEventId).ToList(), request.UserInfo);

        _logger.LogInformation("Accepted Data Locks for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
    }

    private void ReplacePriceHistory(Apprenticeship apprenticeship, List<PriceHistory> updatedPriceHistory, UserInfo userInfo, List<PriceHistory> currentPriceHistory)
    {
        // price history entries are only identified for a given apprenticeship by their from date and to date; 
        // therefore it would be difficult to replace only the ones that have changed when there can be duplicate
        // entries with the same from date and to date
        foreach (var item in apprenticeship.PriceHistory)
        {
            _db.Value.PriceHistory.Remove(item);
        }

        foreach (var item in updatedPriceHistory)
        {
            _db.Value.PriceHistory.Add(item);
        }

        apprenticeship.ReplacePriceHistory(Party.Employer, currentPriceHistory, updatedPriceHistory, userInfo);
    }
}