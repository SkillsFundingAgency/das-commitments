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
        var fieldRows = await db.CocApprovalItems
            .Where(item => item.LearningKey == command.LearningKey)
            .ToListAsync(cancellationToken);

        if (fieldRows.Count == 0)
        {
            return new CocDeleteResult
            {
                Status = DeleteValidationState.NotFound,
                Message = $"An approval was not found in pending state for this learningkey"
            };
        }

        var pendingRows = fieldRows.Where(item => item.Status == CocApprovalItemStatus.Pending).ToList();
        if (pendingRows.Count == 0)
        {
            return new CocDeleteResult
            {
                Status = DeleteValidationState.NotPending,
                Message = $"An approval was found but no pending changes were stored"
            };
        }

        var updated = DateTime.UtcNow;
        foreach (var pendingRow in pendingRows)
        {
            pendingRow.Status = CocApprovalItemStatus.Reversal;
            pendingRow.Updated = updated;
        }

        return new CocDeleteResult
        {
            Status = DeleteValidationState.Cancelled,
            Message = $"An approval was deleted"
        };
    }
}
