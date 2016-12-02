using System;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public interface IApprenticeshipUpdateRules
    {
        AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, bool doChangesRequireAgreement);
        AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, LastAction action);
        CommitmentStatus DetermineNewCommmitmentStatus(bool areAnyApprenticeshipsPendingAgreement);
        EditStatus DetermineNewEditStatus(EditStatus currentEditStatus, CallerType caller, bool areAnyApprenticeshipsPendingAgreement, int apprenticeshipsInCommitment);
        PaymentStatus DetermineNewPaymentStatus(PaymentStatus currentPaymentStatus, bool doChangesRequireAgreement);
        PaymentStatus DetermineNewPaymentStatus(PaymentStatus currentPaymentStatus, AgreementStatus newApprenticeshipAgreementStatus);
        bool DetermineWhetherChangeRequireAgreement(Apprenticeship existingApprenticeship, Apprenticeship updatedApprenticeship);
    }
}
