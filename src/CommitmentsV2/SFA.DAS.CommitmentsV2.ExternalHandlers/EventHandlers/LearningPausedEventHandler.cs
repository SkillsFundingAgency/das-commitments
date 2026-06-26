using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearningPausedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IMessageSession messageSession,
    ILogger<LearningPausedEventHandler> logger)
    : IHandleMessages<LearningPausedEvent>
{
    public async Task Handle(LearningPausedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation(" Started executing {Event}", nameof(LearningPausedEvent));

            if (message is null)
            {
                logger.LogInformation(" {Event} received null message : {Event}", nameof(LearningPausedEvent), message == null);
                return;
            }

            logger.LogInformation("LearningPausedEvent for ApprenticeshipId {ApprenticeshipId} with PauseDate {PauseDate}",
                message.ApprenticeshipId, message.PauseDate);
            var db = dbContext.Value;
            var apprentice = await db.Apprenticeships.Where(t => t.Id == message.ApprenticeshipId).SingleOrDefaultAsync();

            if (apprentice == null)
            {
                throw new DomainException(nameof(apprentice), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
            }

            ValidatePauseDate(message.PauseDate, apprentice);

            apprentice.SetIlrPaused(message.PauseDate);

            var historyCommand = new StoreLearningHistoryCommand
            {
                ApprenticeshipId = message.ApprenticeshipId,
                Source = LearningSourceType.ILRStatusChange,
                ChangeType = LearningChangeType.AutoApproved,
                LearningKey = message.LearningKey,
                AppliedDate = message.Created,
                Description = $"Learning has been paused on {message.PauseDate}"
            };
            await messageSession.Send(historyCommand);

            logger.LogInformation(" Executing {Event} completed", nameof(LearningPausedEvent));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearningPausedEventHandler for ApprenticeshipId {0}", message.ApprenticeshipId);
            throw;
        }
    }

    private void ValidatePauseDate(DateTime pauseDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship.PaymentStatus == PaymentStatus.Completed || apprenticeship.PaymentStatus == PaymentStatus.Withdrawn)
        {
            throw new DomainException(nameof(pauseDate), "Learning cannot be Paused if Payment Status is Completed or Withdrawn. Unable to pause apprenticeship");
        }

        if (apprenticeship.StartDate.HasValue && apprenticeship.StartDate.Value.Date > pauseDate.Date)
        {
            throw new DomainException(nameof(pauseDate), "Invalid pause date. Learner not started.");
        }

        if (apprenticeship.EndDate.HasValue && pauseDate.Date >= apprenticeship.EndDate.Value.Date)
        {
            throw new DomainException(nameof(pauseDate), "Invalid pause date. Pause date cannot be on or after the end date.");
        }
    }
}

// Replace this event with SFA.DAS.Learning.Types
public class LearningPausedEvent
{
    public long ApprenticeshipId { get; set; }
    public DateTime PauseDate { get; set; }
    public Guid LearningKey { get; set; }
    public DateTime Created { get; set; }
}