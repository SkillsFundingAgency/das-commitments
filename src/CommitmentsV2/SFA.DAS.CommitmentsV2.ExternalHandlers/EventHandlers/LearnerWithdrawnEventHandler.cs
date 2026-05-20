using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerWithdrawnEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IMessageSession messageSession,
    ILogger<LearnerWithdrawnEventHandler> logger)
    : IHandleMessages<LearnerWithdrawnEvent>
{
    public async Task Handle(LearnerWithdrawnEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("LearnerWithdrawnEvent for ApprenticeshipId {ApprenticeshipId} with WithdrawnDate {WithdrawnDate} and " +
                "WithdrawnReasonCode {WithdrawnReasonCode}",
                message.ApprenticeshipId, message.WithdrawnDate, message.WithdrawnReasonCode);
            var db = dbContext.Value;
            var apprentice = await db.Apprenticeships.SingleAsync(x => x.Id == message.ApprenticeshipId);
            apprentice.SetIlrWithdrawn(message.WithdrawnDate, message.WithdrawnReasonCode);

            var command = new StoreLearningHistoryCommand
            {
                ApprenticeshipId = message.ApprenticeshipId,
                Source = Types.LearningSourceType.ILRStatusChange,
                ChangeType = Types.LearningChangeType.AutoApproved,
                LearningKey = message.LearningKey,
                AppliedDate = message.Created,
                Description = $"ILR Learner status changed from Live to Withdrawn due to {message.WithdrawnReasonCode}"
            };
            await messageSession.Send(command);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearnerWithdrawnEventHandler for ApprenticeshipId {0}", message.ApprenticeshipId);
            throw;
        }
    }
}

// Will be removed once Learning creates the message
public class LearnerWithdrawnEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime WithdrawnDate { get; set; }
    public int WithdrawnReasonCode { get; set; }
}