using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public interface IApprenticeshipUpdateRules
    {
        AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, bool doChangesRequireAgreement);
        AgreementStatus DetermineNewAgreementStatus(AgreementStatus currentAgreementStatus, CallerType caller, LastAction action);
        CommitmentStatus DetermineNewCommmitmentStatus(bool areAnyApprenticeshipsPendingAgreement);
        EditStatus DetermineNewEditStatus(EditStatus currentEditStatus, CallerType caller, bool areAnyApprenticeshipsPendingAgreement, int apprenticeshipsInCommitment, LastAction lastAction);
        PaymentStatus DetermineNewPaymentStatus(PaymentStatus currentPaymentStatus, bool doChangesRequireAgreement);
        bool DetermineWhetherChangeRequiresAgreement(Apprenticeship existingApprenticeship, Apprenticeship updatedApprenticeship);
    }
}
