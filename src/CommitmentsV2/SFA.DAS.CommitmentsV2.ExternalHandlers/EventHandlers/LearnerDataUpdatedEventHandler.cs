using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.LearnerData.Messages;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerDataUpdatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<LearnerDataUpdatedEventHandler> logger) : IHandleMessages<LearnerDataUpdatedEvent>
{
    public async Task Handle(LearnerDataUpdatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Handling LearnerDataUpdatedEvent for learner {LearnerId} at {ChangedAt}", 
                message.LearnerId, message.ChangedAt);

            await ProcessLearnerDataChanges(message);
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

        var draftApprenticeships = await dbContext.Value.DraftApprenticeships
            .Include(da => da.Cohort)
                .ThenInclude(c => c.TransferRequests)
            .Where(da => da.LearnerDataId == message.LearnerId)
            .ToListAsync();

        if (draftApprenticeships.Count == 0)
        {
            logger.LogInformation("No draft apprenticeship found for learner {LearnerId}", message.LearnerId);
            return;
        }

        foreach (var draftApprenticeship in draftApprenticeships)
        {
            draftApprenticeship.HasLearnerDataChanges = true;
        
            logger.LogInformation("Flagged draft apprenticeship {ApprenticeshipId} for learner data changes", draftApprenticeship.Id);

            var cohort = draftApprenticeship.Cohort;
            if (cohort.WithParty == Party.Employer)
            {
                logger.LogInformation("Cohort {CohortId} is WithEmployer, transitioning back to WithProvider due to learner data changes", cohort.Id);
            
                var systemUserInfo = new UserInfo
                {
                    UserId = "System",
                    UserDisplayName = "System",
                    UserEmail = null
                };

                cohort.SendToOtherParty(Party.Employer, "Cohort returned to provider due to learner data changes requiring updates", systemUserInfo, DateTime.UtcNow);
            
                logger.LogInformation("Successfully transitioned cohort {CohortId} from WithEmployer to WithProvider", cohort.Id);
            }
            else if (cohort.WithParty == Party.TransferSender)
            {
                logger.LogInformation("Cohort {CohortId} is WithTransferSender, transitioning back to WithProvider due to learner data changes", cohort.Id);
            
                var systemUserInfo = new UserInfo
                {
                    UserId = "System",
                    UserDisplayName = "System",
                    UserEmail = null
                };

                var now = DateTime.UtcNow;

                var pendingTransferRequests = cohort.TransferRequests
                    .Where(tr => tr.Status == TransferApprovalStatus.Pending)
                    .ToList();
                
                foreach (var transferRequest in pendingTransferRequests)
                {
                    logger.LogInformation("Silently rejecting TransferRequest {TransferRequestId} for cohort {CohortId} due to learner data changes", 
                        transferRequest.Id, cohort.Id);
                    transferRequest.Reject(systemUserInfo, now, publishEvent: false);
                }

                cohort.SendToOtherParty(Party.TransferSender, "Cohort returned to provider due to learner data changes requiring updates", systemUserInfo, now);
            
                logger.LogInformation("Successfully transitioned cohort {CohortId} from WithTransferSender to WithProvider", cohort.Id);
            }
        }

        await dbContext.Value.SaveChangesAsync();
    }
} 