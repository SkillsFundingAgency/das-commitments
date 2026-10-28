using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcknowledgeInvalidIlrChanges;

public class AcknowledgeInvalidIlrChangesCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDateTime)
    : IRequestHandler<AcknowledgeInvalidIlrChangesCommand>
{
    public const string SelectDeleteMessage = "Select if you would like to delete this alert";

    public async Task Handle(AcknowledgeInvalidIlrChangesCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.Apprenticeships
            .Include(a => a.Cohort)
            .SingleOrDefaultAsync(a => a.Id == command.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            throw new UnauthorizedAccessException($"Apprenticeship {command.ApprenticeshipId} was not found");
        }

        if (apprenticeship.Cohort.ProviderId != command.ProviderId)
        {
            throw new UnauthorizedAccessException($"Provider {command.ProviderId} cannot access apprenticeship {command.ApprenticeshipId}");
        }

        var unacknowledgedRows = await dbContext.Value.CocApprovalItems
            .Where(item => item.ApprenticeshipId == command.ApprenticeshipId
                && item.Status == command.ItemStatus
                && item.ProviderAcknowledgedAt == null)
            .ToListAsync(cancellationToken);

        var acknowledgements = command.Acknowledgements ?? [];
        var errors = new List<DomainError>();

        for (var index = 0; index < unacknowledgedRows.Count; index++)
        {
            var fieldRow = unacknowledgedRows[index];
            var acknowledgement = acknowledgements.SingleOrDefault(item => item.ApprovalRequestId == fieldRow.Id);

            if (acknowledgement == null || !acknowledgement.DeleteAlert.HasValue)
            {
                errors.Add(new DomainError($"RequestSets[{index}].DeleteAlert", SelectDeleteMessage));
            }
        }

        if (errors.Count > 0)
        {
            throw new DomainException(errors);
        }

        var userId = command.UserInfo?.UserId;
        var acknowledgedAt = currentDateTime.UtcNow;

        foreach (var fieldRow in unacknowledgedRows)
        {
            var acknowledgement = acknowledgements.Single(item => item.ApprovalRequestId == fieldRow.Id);
            if (acknowledgement.DeleteAlert == true)
            {
                fieldRow.ProviderAcknowledgedBy = userId;
                fieldRow.ProviderAcknowledgedAt = acknowledgedAt;
            }
        }

        await dbContext.Value.SaveChangesAsync(cancellationToken);
    }
}
