using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerDataUpdatedEventHandler(ILogger<LearnerDataUpdatedEventHandler> logger) : IHandleMessages<LearnerDataUpdatedEvent>
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

                // TODO: Implement business logic to process learner data changes
                // This could include:
                // - Updating apprenticeship records
                // - Triggering notifications
                // - Updating related entities
                // - Logging changes for audit purposes
                
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

    protected virtual async Task ProcessLearnerDataChanges(LearnerDataUpdatedEvent message)
    {
        // TODO: Implement the actual business logic for processing learner data changes
        // This is a placeholder for the actual implementation
        await Task.CompletedTask;
    }
} 