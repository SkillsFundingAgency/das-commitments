using System;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public class ApprenticeshipUpdateRules : IApprenticeshipUpdateRules
    {
        public AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, bool doChangesRequireAgreement)
        {
            if (!doChangesRequireAgreement) return currentAgreementStatus;

            return AgreementStatus.NotAgreed;
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

        public EditStatus DetermineNewEditStatus(EditStatus currentEditStatus, CallerType caller, bool areAnyApprenticeshipsPendingAgreement, int apprenticeshipsInCommitment)
        {
            if (areAnyApprenticeshipsPendingAgreement)
                return caller == CallerType.Provider ? EditStatus.EmployerOnly : EditStatus.ProviderOnly;

            return apprenticeshipsInCommitment > 0 ? EditStatus.Both : currentEditStatus;
        }

        public AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, LastAction action)
        {
            if (action == LastAction.Amend)
            {
                if (!CallerApproved(currentAgreementStatus, caller))
                {
                    return AgreementStatus.NotAgreed;
                }

                return currentAgreementStatus;
            }

            switch (caller)
            {
                case CallerType.Employer:
                    if (currentAgreementStatus == AgreementStatus.ProviderAgreed)
                        return AgreementStatus.BothAgreed;
                    if (currentAgreementStatus == AgreementStatus.NotAgreed)
                        return AgreementStatus.EmployerAgreed;
                    break;
                case CallerType.Provider:
                    if (currentAgreementStatus == AgreementStatus.EmployerAgreed)
                        return AgreementStatus.BothAgreed;
                    if (currentAgreementStatus == AgreementStatus.NotAgreed)
                        return AgreementStatus.ProviderAgreed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            throw new ArgumentException($"Invalid combination of values - CurrentAgreementStatus:{currentAgreementStatus}, Caller:{caller}, Action:{action}");
        }

        private bool CallerApproved(AgreementStatus currentAgreementStatus, CallerType caller)
        {
            switch (caller)
            {
                case CallerType.Employer:
                    return currentAgreementStatus == AgreementStatus.EmployerAgreed;
                case CallerType.Provider:
                    return currentAgreementStatus == AgreementStatus.ProviderAgreed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(caller), caller, null);
            }
        }
    }
}
