using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerDataUpdatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<LearnerDataUpdatedEventHandler> logger) : IHandleMessages<LearnerDataUpdatedEvent>
{
    public async Task Handle(LearnerDataUpdatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Handling LearnerDataUpdatedEvent for learner {LearnerId} with {ChangeCount} changes", 
                message.LearnerId, message.ChangeSummary.Changes.Count);

            if (message.ChangeSummary.HasChanges)
            {
                foreach (var change in message.ChangeSummary.Changes)
                {
                    logger.LogInformation("Learner {LearnerId} field '{FieldName}' changed from '{OldValue}' to '{NewValue}'", 
                        message.LearnerId, change.FieldName, change.OldValue, change.NewValue);
                }

                await ProcessLearnerDataChanges(message);
            }
            else
            {
                logger.LogInformation("LearnerDataUpdatedEvent for learner {LearnerId} has no changes to process", message.LearnerId);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearnerDataUpdatedEvent for learner {LearnerId}", message.LearnerId);
            throw;
        }
    }

    public async Task ProcessLearnerDataChanges(LearnerDataUpdatedEvent message)
    {
        logger.LogInformation("Processing learner data changes for learner {LearnerId}", message.LearnerId);

        if (!message.ChangeSummary.HasChanges)
        {
            logger.LogInformation("No changes detected for learner {LearnerId}, skipping processing", message.LearnerId);
            return;
        }

        var draftApprenticeship = await dbContext.Value.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        if (draftApprenticeship == null)
        {
            logger.LogWarning("No draft apprenticeship found for learner {LearnerId}", message.LearnerId);
            return;
        }

        draftApprenticeship.HasLearnerDataChanges = true;
        draftApprenticeship.LastLearnerDataSync = DateTime.UtcNow;
        logger.LogInformation("Flagged draft apprenticeship {ApprenticeshipId} for learner data changes", draftApprenticeship.Id);

        await dbContext.Value.SaveChangesAsync();
        logger.LogInformation("Successfully updated draft apprenticeship {ApprenticeshipId} for learner {LearnerId}", draftApprenticeship.Id, message.LearnerId);
    }
} 