using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed
{
    public class ApprenticeshipConfirmedCommandHandler : AsyncRequestHandler<ApprenticeshipConfirmedCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public ApprenticeshipConfirmedCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        protected override async Task Handle(ApprenticeshipConfirmedCommand request, CancellationToken cancellationToken)
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