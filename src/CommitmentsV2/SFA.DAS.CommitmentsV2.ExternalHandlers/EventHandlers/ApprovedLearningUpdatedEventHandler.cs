using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.ExternalHandlers.LearningEvents;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class ApprovedLearningUpdatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprovedLearningUpdatedEventHandler> logger)

{
    public async Task Handle(ApprovedLearningUpdatedEvent message)
    {
        try
        {
            logger.LogInformation("Started executing {Event}", nameof(ApprovedLearningUpdatedEvent));

            if (message is null)
            {
                logger.LogInformation(" {Event} received null message : {Event}", nameof(ApprovedLearningUpdatedEvent), message == null);
                return;
            }

            logger.LogInformation("ApprovedLearningUpdatedEvent for ApprenticeshipId {ApprenticeshipId} with LearningKey {LearningKey}",
                message.ApprenticeshipId, message.LearningKey);
            var db = dbContext.Value;
            var apprentice = await db.Apprenticeships
                .Include(a => a.Cohort)
                    .ThenInclude(c => c.Provider)
                .SingleOrDefaultAsync(t => t.Id == message.ApprenticeshipId);

            if (apprentice == null)
            {
                throw new DomainException(nameof(apprentice), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
            }

            foreach (var change in message.Changes)
            {
                if (change?.Data == null) continue;

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
                            apprentice.DateOfBirth = ParseDate(change.Data.New);
                            break;

                        case ApprovedLearnerChangeType.PlannedStartDate:
                            apprentice.StartDate = ParseDate(change.Data.New);
                            break;

                        case ApprovedLearnerChangeType.PlannedEndDate:
                            apprentice.EndDate = ParseDate(change.Data.New);
                            break;

                        case ApprovedLearnerChangeType.Email:
                            apprentice.Email = change.Data.New;
                            break;
                    }
                }
                else
                {
                    logger.LogWarning("Unknown change type '{ChangeType}' for ApprenticeshipId {ApprenticeshipId}", change.ChangeType, message.ApprenticeshipId);
                }
            }

            logger.LogInformation(" Executing {Event} completed", nameof(ApprovedLearningUpdatedEvent));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing ApprovedLearningUpdatedEventHandler for ApprenticeshipId {0}", message.ApprenticeshipId);
            throw;
        }
    }

    private static DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            return parsedDate;
        }
        return null;
    }
}