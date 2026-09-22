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
           .Include(a => a.ApprovalRequests)
           .SingleOrDefaultAsync(a => a.Id == command.ApprenticeshipId, cancellationToken);

            if (apprenticeship == null)
            {
                throw new UnauthorizedAccessException($"Apprenticeship {command.ApprenticeshipId} was not found");
            }

            if (apprenticeship.Cohort.EmployerAccountId != command.AccountId)
            {
                throw new UnauthorizedAccessException($"Employer {command.AccountId} cannot access apprenticeship {command.ApprenticeshipId}");
            }

            var approvalRequests = (apprenticeship.ApprovalRequests ?? [])
                .Where(ar => ar.ApprenticeshipId == command.ApprenticeshipId
                && ar.EmployerAcknowledgedBy == null && ar.EmployerAcknowledgedAt == null).ToList();

            foreach (var approvalRequest in approvalRequests)
            {
                var request = command.ApprovalRequests.FirstOrDefault(ar => ar.ApprovalRequestId == approvalRequest.Id && ar.Acknowledged == true);
                if (request == null)
                {
                    continue;
                }
                approvalRequest.EmployerAcknowledgedBy = request?.UserInfo?.UserId;
                approvalRequest.EmployerAcknowledgedAt = DateTime.UtcNow;
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