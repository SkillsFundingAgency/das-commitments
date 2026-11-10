using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprovalRequestAlertSeen;

public class UpdateApprovalRequestAlertAcknowledgeCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<UpdateApprovalRequestAlertAcknowledgeCommandHandler> logger)
    : IRequestHandler<UpdateApprovalRequestAlertAcknowledgeCommand>
{
    public async Task Handle(UpdateApprovalRequestAlertAcknowledgeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("UpdateApprovalRequestAlertAcknowledgeCommand  called for ApprenticeshipId : {ApprenticeshipId} ", command.ApprenticeshipId);

            var apprenticeship = await dbContext.Value.Apprenticeships
           .Include(a => a.Cohort)
           .SingleOrDefaultAsync(a => a.Id == command.ApprenticeshipId, cancellationToken);

            if (apprenticeship == null)
            {
                throw new UnauthorizedAccessException($"Apprenticeship {command.ApprenticeshipId} was not found");
            }

            if (apprenticeship.Cohort.EmployerAccountId != command.AccountId)
            {
                throw new UnauthorizedAccessException($"Employer {command.AccountId} cannot access apprenticeship {command.ApprenticeshipId}");
            }

            var acknowledgedIds = (command.ApprovalRequests ?? [])
                .Where(alert => alert.Acknowledged)
                .Select(alert => alert.ApprovalRequestId)
                .ToList();

            var fieldRows = await dbContext.Value.CocApprovalItems
                .Where(item => item.ApprenticeshipId == command.ApprenticeshipId
                    && item.Status == Models.CocApprovalItemStatus.AutoApproved
                    && item.EmployerAcknowledgedAt == null
                    && acknowledgedIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

            foreach (var fieldRow in fieldRows)
            {
                var request = command.ApprovalRequests.First(alert => alert.ApprovalRequestId == fieldRow.Id);
                fieldRow.EmployerAcknowledgedBy = request.UserInfo?.UserId;
                fieldRow.EmployerAcknowledgedAt = DateTime.UtcNow;
            }

            await dbContext.Value.SaveChangesAsync(cancellationToken);

            logger.LogInformation("UpdateApprovalRequestAlertAcknowledgeCommand completed for ApprenticeshipId : {ApprenticeshipId} ", command.ApprenticeshipId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error Updating Approval Request Alert Acknowledge for Apprenticeship with id {ApprenticeshipId}", command.ApprenticeshipId);
            throw;
        }
    }
}