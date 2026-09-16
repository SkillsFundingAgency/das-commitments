using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class ApprovedLearningUpdatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
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
                .SingleOrDefaultAsync(t => t.Id == message.ApprenticeshipId);

            if (apprentice == null)
            {
                throw new DomainException(nameof(apprentice), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
            }

            foreach (var change in message.Changes)
            {
                ApplyChange(message, apprentice, change);
            }
            await db.SaveChangesAsync();

            var @event = await CreateApprenticeshipUpdatedApprovedEvent(message.ApprenticeshipId);
            logger.LogInformation("Publishing ApprenticeshipUpdatedApprovedEvent");
            await context.Publish(@event);

            logger.LogInformation(" Executing {Event} completed", nameof(ApprovedLearningUpdatedEvent));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing ApprovedLearningUpdatedEventHandler for ApprenticeshipId {0}", message?.ApprenticeshipId);
            throw;
        }
    }

    private async Task<ApprenticeshipUpdatedApprovedEvent> CreateApprenticeshipUpdatedApprovedEvent(long apprenticeshipId)
    {
        logger.LogInformation("Refetching Apprenticeship {id} so we can send ApprenticeshipUpdatedApprovedEvent", apprenticeshipId);

        var apprenticeship = await dbContext.Value.Apprenticeships
                .Include(a => a.PriceHistory)
                .Include(b => b.FlexibleEmployment)
            .SingleOrDefaultAsync(a => a.Id == apprenticeshipId);
        var course = dbContext.Value.Courses.FirstOrDefault(x => x.LarsCode == apprenticeship.CourseCode);

        return new ApprenticeshipUpdatedApprovedEvent
        {
            ApprenticeshipId = apprenticeship.Id,
            StandardUId = apprenticeship.StandardUId,
            TrainingCourseVersion = apprenticeship.TrainingCourseVersion,
            TrainingCourseOption = apprenticeship.TrainingCourseOption,
            ApprovedOn = DateTime.UtcNow,
            Uln = apprenticeship.Uln,
            StartDate = apprenticeship.StartDate ?? DateTime.MinValue,
            EndDate = apprenticeship.EndDate ?? DateTime.MinValue,
            PriceEpisodes = apprenticeship.PriceHistory.Select(ph => new PriceEpisode
            {
                FromDate = ph.FromDate,
                ToDate = ph.ToDate,
                Cost = ph.Cost,
                TrainingPrice = ph.TrainingPrice,
                EndPointAssessmentPrice = ph.AssessmentPrice
            }).ToArray(),
            TrainingType = (ProgrammeType)apprenticeship.ProgrammeType,
            TrainingCode = apprenticeship.CourseCode,
            DeliveryModel = apprenticeship.DeliveryModel ?? DeliveryModel.Regular,
            LearningType = (Common.Domain.Types.LearningType)(course?.LearningType ?? LearningType.Apprenticeship),
            EmploymentEndDate = apprenticeship.FlexibleEmployment?.EmploymentEndDate,
            EmploymentPrice = apprenticeship.FlexibleEmployment?.EmploymentPrice
        };
    }

    private void ApplyChange(ApprovedLearningUpdatedEvent message, Models.Apprenticeship apprentice, ApprenticeshipFieldChange change)
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

                case ApprovedLearnerChangeType.PlannedStartDate:
                    var parsedStartDate = ParseDate(change.Data.New);
                    if (parsedStartDate == null)
                    {
                        logger.LogWarning("Invalid date for PlannedStartDate change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
                        return;
                    }

                    apprentice.StartDate = ParseFirstDayOfMonth(parsedStartDate);
                    apprentice.ActualStartDate = parsedStartDate;
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

        return;
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
        return;
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