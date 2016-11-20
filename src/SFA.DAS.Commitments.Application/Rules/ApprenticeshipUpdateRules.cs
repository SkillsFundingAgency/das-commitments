using System;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    //todo: add unit tests for all updates rules
    public class ApprenticeshipUpdateRules : IApprenticeshipUpdateRules
    {
        public AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, bool doChangesRequireAgreement)
        {
            if (!doChangesRequireAgreement) return currentAgreementStatus;

            return caller == CallerType.Employer ? AgreementStatus.EmployerAgreed : AgreementStatus.ProviderAgreed;
        }

        public PaymentStatus DetermineNewPaymentStatus(PaymentStatus currentPaymentStatus, bool doChangesRequireAgreement)
        {
            return doChangesRequireAgreement ? PaymentStatus.PendingApproval : currentPaymentStatus;
        }

        public bool DetermineWhetherChangeRequireAgreement(Apprenticeship existingApprenticeship, Apprenticeship updatedApprenticeship)
        {
            if (existingApprenticeship.Cost != updatedApprenticeship.Cost) return true;
            if (existingApprenticeship.DateOfBirth != updatedApprenticeship.DateOfBirth) return true;
            if (existingApprenticeship.FirstName != updatedApprenticeship.FirstName) return true;
            if (existingApprenticeship.LastName != updatedApprenticeship.LastName) return true;
            if (existingApprenticeship.NINumber != updatedApprenticeship.NINumber) return true;
            if (existingApprenticeship.StartDate != updatedApprenticeship.StartDate) return true;
            if (existingApprenticeship.EndDate != updatedApprenticeship.EndDate) return true;
            if (existingApprenticeship.TrainingCode != updatedApprenticeship.TrainingCode) return true;
            if (existingApprenticeship.TrainingType != updatedApprenticeship.TrainingType) return true;
            if (existingApprenticeship.TrainingName != updatedApprenticeship.TrainingName) return true;

            return false;
        }

        public CommitmentStatus DetermineNewCommmitmentStatus(bool areAnyApprenticeshipsPendingAgreement)
        {
            //todo: commitment status will be set to "deleted" if all apprenticeships are agreed (after private beta wave 2b.1)
            return areAnyApprenticeshipsPendingAgreement ? CommitmentStatus.Active : CommitmentStatus.Active;
        }

        public PaymentStatus DetermineNewPaymentStatus(PaymentStatus currentPaymentStatus, AgreementStatus newApprenticeshipAgreementStatus)
        {
            switch (currentPaymentStatus)
            {
                case PaymentStatus.PendingApproval:
                case PaymentStatus.Active:
                case PaymentStatus.Paused:
                    return newApprenticeshipAgreementStatus == AgreementStatus.BothAgreed ? PaymentStatus.Active : PaymentStatus.PendingApproval;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentPaymentStatus), currentPaymentStatus, null);
            }
        }

        public EditStatus DetermineNewEditStatus(CallerType caller, bool areAnyApprenticeshipsPendingAgreement)
        {
            if (areAnyApprenticeshipsPendingAgreement)
                return caller == CallerType.Provider ? EditStatus.EmployerOnly : EditStatus.ProviderOnly;

            return EditStatus.Both;
        }

        public AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, AgreementStatus newAgreementStatus)
        {
            switch (newAgreementStatus)
            {
                case AgreementStatus.NotAgreed:
                    return AgreementStatus.NotAgreed;

                case AgreementStatus.EmployerAgreed:
                case AgreementStatus.ProviderAgreed:
                    switch (currentAgreementStatus)
                    {
                        case AgreementStatus.NotAgreed:
                        case AgreementStatus.BothAgreed:
                            return newAgreementStatus;

                        case AgreementStatus.EmployerAgreed:
                            return caller == CallerType.Employer ? AgreementStatus.EmployerAgreed : AgreementStatus.BothAgreed;

                        case AgreementStatus.ProviderAgreed:
                            return caller == CallerType.Employer ? AgreementStatus.BothAgreed : AgreementStatus.ProviderAgreed;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(currentAgreementStatus), currentAgreementStatus, null);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(newAgreementStatus), newAgreementStatus, null);
            }
        }

    }
}
