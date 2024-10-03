using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;

public class ApprenticeshipConfirmationCommencedCommandHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<ApprenticeshipConfirmationCommencedCommand>
{
    public async Task Handle(ApprenticeshipConfirmationCommencedCommand request, CancellationToken cancellationToken)
    {
        var status = await db.Value.ApprenticeshipConfirmationStatus.SingleOrDefaultAsync(a => a.ApprenticeshipId == request.ApprenticeshipId, cancellationToken);

        if (status == null)
        {
            var confirmationStatus = new ApprenticeshipConfirmationStatus(request.ApprenticeshipId, request.CommitmentsApprovedOn, request.ConfirmationOverdueOn, null);
            db.Value.ApprenticeshipConfirmationStatus.Add(confirmationStatus);
        }
        else
        {
            status.SetStatusToUnconfirmedIfChangeIsLatest(request.CommitmentsApprovedOn, request.ConfirmationOverdueOn);
        }
    }
}