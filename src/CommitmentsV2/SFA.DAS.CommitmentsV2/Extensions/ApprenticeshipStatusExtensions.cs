using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class ApprenticeshipStatusExtensions
{
    public static PaymentStatus MapToPaymentStatus(this ApprenticeshipStatus status)
    {
        switch (status)
        {
            case ApprenticeshipStatus.WaitingToStart:
            case ApprenticeshipStatus.Live:
                return PaymentStatus.Active;
            case ApprenticeshipStatus.Paused:
                return PaymentStatus.Paused;
            case ApprenticeshipStatus.Stopped:
                return PaymentStatus.Withdrawn;
            case ApprenticeshipStatus.Completed:
                return PaymentStatus.Completed;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}