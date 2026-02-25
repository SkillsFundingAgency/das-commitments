using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipCreatedEmployerVerificationEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprenticeshipCreatedEmployerVerificationEventHandler> logger)
    : IHandleMessages<ApprenticeshipCreatedEvent>
{
    public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
    {
        var db = dbContext.Value;
        var existing = await db.EmployerVerificationRequests.FindAsync(message.ApprenticeshipId);
        if (existing != null)
        {
            logger.LogInformation("EmployerVerificationRequest already exists for Apprenticeship {ApprenticeshipId}, skipping.", message.ApprenticeshipId);
            return;
        }

        logger.LogInformation("Creating EmployerVerificationRequest for Apprenticeship {ApprenticeshipId}", message.ApprenticeshipId);
        db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = message.ApprenticeshipId,
            Created = DateTime.UtcNow,
            Status = EmployerVerificationRequestStatus.Pending
        });
        
        await db.SaveChangesAsync();
    }
}
