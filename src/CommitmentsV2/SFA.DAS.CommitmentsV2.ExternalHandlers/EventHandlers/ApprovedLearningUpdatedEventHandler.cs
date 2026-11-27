using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class ApprovedLearningUpdatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ITrainingProgrammeLookup trainingProgrammeLookup,
    IApprovalsOuterApiClient approvalsOuterApiClient,
    ICurrentDateTime currentDateTime,
    ILogger<ApprovedLearningUpdatedEventHandler> logger) : IHandleMessages<ApprovedLearningUpdatedEvent>

{
    public async Task Handle(ApprovedLearningUpdatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Started executing {Event}", nameof(ApprovedLearningUpdatedEvent));

            if (message is null)
            {
                logger.LogInformation(" {Event} received null message", nameof(ApprovedLearningUpdatedEvent));
                return;
            }

            logger.LogInformation("ApprovedLearningUpdatedEvent for ApprenticeshipId {ApprenticeshipId} with LearningKey {LearningKey}",
                message.ApprenticeshipId, message.LearningKey);
            var db = dbContext.Value;
            var apprentice = await db.Apprenticeships
                .Include(t => t.PriceHistory)
                .SingleOrDefaultAsync(t => t.Id == message.ApprenticeshipId);

            if (apprentice == null)
            {
                throw new DomainException(nameof(apprentice), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
            }

            foreach (var change in message.Changes ?? [])
            {
                await ApplyChange(message, apprentice, change);
            }

            var latestPrice = message.Prices?
                .OrderBy(price => price.EffectiveFrom)
                .LastOrDefault();

            if (latestPrice != null)
            {
                apprentice.ApplyLatestApprovedPrice(
                    latestPrice.TrainingPrice,
                    latestPrice.AssessmentPrice,
                    latestPrice.EffectiveFrom);
            }

            logger.LogInformation(" Executing {Event} completed", nameof(ApprovedLearningUpdatedEvent));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing ApprovedLearningUpdatedEventHandler for ApprenticeshipId {0}", message?.ApprenticeshipId);
            throw;
        }
    }

    private async Task ApplyChange(ApprovedLearningUpdatedEvent message, Apprenticeship apprentice, ApprenticeshipFieldChange change)
    {
        if (change?.Data == null) return;

        if (Enum.TryParse<ApprovedLearnerChangeType>(change.ChangeType, ignoreCase: true, out var changeType))
        {
            switch (changeType)
            {
                case ApprovedLearnerChangeType.Firstname:
                    apprentice.FirstName = change.Data.New;
                    break;

                case ApprovedLearnerChangeType.Surname:
                    apprentice.LastName = change.Data.New;
                    break;

                case ApprovedLearnerChangeType.DOB:
                    ApplyDOBChange(message, apprentice, change);
                    break;

                case ApprovedLearnerChangeType.StartDate:
                    var parsedStartDate = ParseDate(change.Data.New);
                    if (parsedStartDate == null)
                    {
                        logger.LogWarning("Invalid date for StartDate change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
                        return;
                    }

                    apprentice.StartDate = ParseFirstDayOfMonth(parsedStartDate);
                    apprentice.ActualStartDate = parsedStartDate;
                    if (apprentice.PriceHistory.Count == 1)
                    {
                        apprentice.PriceHistory.Single().FromDate = apprentice.StartDate.Value;
                    }

                    await RecalculateCourseVersion(apprentice, message.LearningType, parsedStartDate.Value);
                    break;

                case ApprovedLearnerChangeType.PlannedEndDate:
                    var parsedEndDate = ParseDate(change.Data.New);
                    if (parsedEndDate == null)
                    {
                        logger.LogWarning("Invalid date for PlannedEndDate change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
                        return;
                    }
                    apprentice.EndDate = ParseFirstDayOfMonth(parsedEndDate);
                    break;

                case ApprovedLearnerChangeType.Email:
                    apprentice.Email = change.Data.New;
                    break;
            }
        }
        else
        {
            logger.LogWarning("Unknown change type '{ChangeType}' for ApprenticeshipId {ApprenticeshipId}", change.ChangeType, message?.ApprenticeshipId);
        }
    }

    private async Task RecalculateCourseVersion(Apprenticeship apprentice, string learningType, DateTime startDate)
    {
        if (apprentice.GetApprenticeshipStatus(currentDateTime.UtcNow) != ApprenticeshipStatus.WaitingToStart ||
            string.IsNullOrWhiteSpace(apprentice.CourseCode))
        {
            return;
        }

        var (version, standardUId) = await ResolveCourseVersion(apprentice.CourseCode, learningType, startDate);
        if (string.IsNullOrWhiteSpace(version) ||
            string.IsNullOrWhiteSpace(standardUId) ||
            version == apprentice.TrainingCourseVersion)
        {
            return;
        }

        apprentice.TrainingCourseVersion = version;
        apprentice.StandardUId = standardUId;
        apprentice.TrainingCourseOption = null;
    }

    private async Task<(string Version, string StandardUId)> ResolveCourseVersion(
        string courseCode, string learningType, DateTime startDate)
    {
        if (string.Equals(learningType, nameof(LearningType.ApprenticeshipUnit), StringComparison.OrdinalIgnoreCase))
        {
            var unit = await approvalsOuterApiClient.Get<GetApprenticeshipUnitVersionResponse>(
                new GetApprenticeshipUnitVersionRequest(courseCode, startDate));
            return (unit?.Version, unit?.StandardUId);
        }

        var standard = await trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(courseCode, startDate);
        if (standard?.VersionEarliestStartDate > startDate || standard?.VersionLatestStartDate < startDate)
        {
            return (null, null);
        }

        return (standard?.Version, standard?.StandardUId);
    }

    private void ApplyDOBChange(ApprovedLearningUpdatedEvent message, Models.Apprenticeship apprentice, ApprenticeshipFieldChange change)
    {
        var parsedDOB = ParseDate(change.Data.New);
        if (parsedDOB == null)
        {
            logger.LogWarning("Invalid date for DOB change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
            return;
        }
        apprentice.DateOfBirth = parsedDOB;
    }

    private static DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate;
        }
        return null;
    }

    private static DateTime? ParseFirstDayOfMonth(DateTime? date)
    {
        return date.HasValue ? new DateTime(date.Value.Year, date.Value.Month, 1) : null;
    }
}
