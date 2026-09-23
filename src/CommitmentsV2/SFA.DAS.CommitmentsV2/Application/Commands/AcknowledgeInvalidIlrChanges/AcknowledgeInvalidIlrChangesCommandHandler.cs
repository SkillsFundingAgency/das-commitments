using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
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
            .Include(a => a.ApprovalRequests)
            .ThenInclude(request => request.Items)
            .SingleOrDefaultAsync(a => a.Id == command.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            throw new UnauthorizedAccessException($"Apprenticeship {command.ApprenticeshipId} was not found");
        }

        if (apprenticeship.Cohort.ProviderId != command.ProviderId)
        {
            throw new UnauthorizedAccessException($"Provider {command.ProviderId} cannot access apprenticeship {command.ApprenticeshipId}");
        }

        var unacknowledgedRequests = (apprenticeship.ApprovalRequests ?? [])
            .Where(request => request.IsUnacknowledgedAutoRejected())
            .ToList();

        var acknowledgements = command.Acknowledgements ?? [];
        var errors = new List<DomainError>();

        for (var index = 0; index < unacknowledgedRequests.Count; index++)
        {
            var request = unacknowledgedRequests[index];
            var acknowledgement = acknowledgements.SingleOrDefault(item => item.ApprovalRequestId == request.Id);

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

        foreach (var request in unacknowledgedRequests)
        {
            var acknowledgement = acknowledgements.Single(item => item.ApprovalRequestId == request.Id);
            if (acknowledgement.DeleteAlert == true)
            {
                request.AcknowledgeByProvider(userId, acknowledgedAt);
            }
        }

        await dbContext.Value.SaveChangesAsync(cancellationToken);
    }
}
