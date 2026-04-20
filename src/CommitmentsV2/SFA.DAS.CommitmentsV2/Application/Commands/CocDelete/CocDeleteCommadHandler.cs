using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocDelete;

public class CocDeleteCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<CocDeleteCommandHandler> logger)
    : IRequestHandler<CocDeleteCommand, CocDeleteResult>
{
    public async Task<CocDeleteResult> Handle(CocDeleteCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("=== Coc Delete command handler called ===");

        ArgumentNullException.ThrowIfNull(command);

        var db = dbContext.Value;
        var latestApprovalRequest = await db.ApprovalRequests.Where(r => r.LearningKey == command.LearningKey)
                     .OrderByDescending(r => r.Created)
                     .FirstOrDefaultAsync(cancellationToken);

        if (latestApprovalRequest is null)
        {
            return new CocDeleteResult
            {
                Status = DeleteValidationState.NotFound,
                Message = $"An approval was not found in pending state for  this learningkey"
            };
        }

        if (latestApprovalRequest.Status != CocApprovalResultStatus.Pending)
        {
            return new CocDeleteResult
            {
                Status = DeleteValidationState.NotPending,
                Message = $"An approval was found but no pending changes were stored"
            };
        }

        latestApprovalRequest.Status = CocApprovalResultStatus.Cancelled;

        db.ApprovalRequests.Update(latestApprovalRequest);

        return new CocDeleteResult
        {
            Status = DeleteValidationState.Cancelled,
            Message = $"An approval was deleted"
        };
    }
}