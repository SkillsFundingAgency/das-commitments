using System;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Mappers
{
    public interface IApprenticeshipStatusMapper
    {
        ApprenticeshipStatus MapPaymentStatus(PaymentStatus paymentStatus, DateTime? apprenticeshipStartDate);
    }
    public class ApprenticeshipStatusMapper : IApprenticeshipStatusMapper
    {
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipStatusMapper(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public ApprenticeshipStatus MapPaymentStatus(PaymentStatus paymentStatus, DateTime? apprenticeshipStartDate)
        {
            var now = new DateTime(_currentDateTime.UtcNow.Year, _currentDateTime.UtcNow.Month, 1);
            var waitingToStart = apprenticeshipStartDate.HasValue && apprenticeshipStartDate.Value > now;

            switch (paymentStatus)
            {
                case PaymentStatus.Active:
                    return waitingToStart ? ApprenticeshipStatus.WaitingToStart : ApprenticeshipStatus.Live;
                case PaymentStatus.Paused:
                    return ApprenticeshipStatus.Paused;
                case PaymentStatus.Withdrawn:
                    return ApprenticeshipStatus.Stopped;
                case PaymentStatus.Completed:
                    return ApprenticeshipStatus.Finished;
                case PaymentStatus.Deleted:
                    return ApprenticeshipStatus.Live;
                default:
                    return ApprenticeshipStatus.WaitingToStart;
            }
        }
    }
}