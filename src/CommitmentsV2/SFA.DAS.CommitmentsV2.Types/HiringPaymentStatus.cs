namespace SFA.DAS.CommitmentsV2.Types;

public enum HiringPaymentStatus :short
{
    // Pre-employment verification is in progress.
    QualifiesForHiringPayment = 1,
    PassedPreEmploymentCheck = 2,
    FailedPreEmploymentCheck = 3,
    EligibleForFirstPayment = 4,
    IneligibleForFirstPayment = 5,
    EligibleForSecondPayment = 6,
    IneligibleForSecondPayment = 7
}