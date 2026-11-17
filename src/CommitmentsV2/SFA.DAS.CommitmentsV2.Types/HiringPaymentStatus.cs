namespace SFA.DAS.CommitmentsV2.Types;

public enum HiringPaymentStatus :short
{
    QualifiesForHiringPayment = 1,
    PassedPreEmploymentCheck = 2,
    FailedPreEmploymentCheck = 3,
    EligibleForFirstPayment = 4,
    IneligibleForFirstPayment = 5,
    EligibleForSecondPayment = 6,
    IneligibleForSecondPayment = 7
}