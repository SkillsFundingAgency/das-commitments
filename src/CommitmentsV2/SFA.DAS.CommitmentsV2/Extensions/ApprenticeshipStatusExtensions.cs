using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class ApprenticeshipStatusExtensions
    {
        public static PaymentStatus[] MapToPaymentStatuses(this ApprenticeshipStatus status)
        {
            switch (status)
            {
                case ApprenticeshipStatus.WaitingToStart:
                    return new[] {PaymentStatus.PendingApproval, PaymentStatus.Active};

                case ApprenticeshipStatus.Live:
                    return new[] {PaymentStatus.Active, PaymentStatus.Deleted};

                case ApprenticeshipStatus.Paused:
                    return new[] {PaymentStatus.Paused};

                case ApprenticeshipStatus.Stopped:
                    return new[] {PaymentStatus.Withdrawn};

                case ApprenticeshipStatus.Completed:
                    return new[] {PaymentStatus.Completed};

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
