using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed
{
    public class ApprenticeshipConfirmedCommandHandler : IRequestHandler<ApprenticeshipConfirmedCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public ApprenticeshipConfirmedCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task Handle(ApprenticeshipConfirmedCommand request, CancellationToken cancellationToken)
        {
            var status = await _db.Value.ApprenticeshipConfirmationStatus.SingleOrDefaultAsync(a => a.ApprenticeshipId == request.ApprenticeshipId, cancellationToken);

            if (status == null)
            {
                var confirmationStatus = new ApprenticeshipConfirmationStatus(request.ApprenticeshipId, request.CommitmentsApprovedOn, null, request.ConfirmedOn);
                _db.Value.ApprenticeshipConfirmationStatus.Add(confirmationStatus);
            }
            else
            {
                status.SetStatusToConfirmedIfChangeIsLatest(request.CommitmentsApprovedOn, request.ConfirmedOn);
            }
        }
    }
}