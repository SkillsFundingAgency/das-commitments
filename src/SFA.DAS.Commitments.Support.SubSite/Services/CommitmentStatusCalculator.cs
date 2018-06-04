using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Enums;

namespace SFA.DAS.Commitments.Support.SubSite.Services
{
    public sealed class CommitmentStatusCalculator : ICommitmentStatusCalculator
    {
        public RequestStatus GetStatus(EditStatus editStatus, int apprenticeshipCount, LastAction lastAction, AgreementStatus? overallAgreementStatus)
        {
            bool hasApprenticeships = apprenticeshipCount > 0;

            if (editStatus == EditStatus.Both)
                return RequestStatus.Approved;

            if (editStatus == EditStatus.ProviderOnly)
            {
                return GetProviderOnlyStatus(lastAction, hasApprenticeships);
            }

            if (editStatus == EditStatus.EmployerOnly)
            {
                return GetEmployerOnlyStatus(lastAction, hasApprenticeships, overallAgreementStatus);
            }

            return RequestStatus.None;
        }

        private static RequestStatus GetProviderOnlyStatus(LastAction lastAction, bool hasApprenticeships)
        {
            if (!hasApprenticeships || lastAction == LastAction.None)
                return RequestStatus.SentToProvider;

            if (lastAction == LastAction.Amend)
                return RequestStatus.SentForReview;

            if (lastAction == LastAction.Approve)
                return RequestStatus.WithProviderForApproval;

            return RequestStatus.None;
        }

        private RequestStatus GetEmployerOnlyStatus(LastAction lastAction, bool hasApprenticeships, AgreementStatus? overallAgreementStatus)
        {
            if (!hasApprenticeships || lastAction == LastAction.None)
                return RequestStatus.NewRequest;

            if (lastAction >= LastAction.Amend && overallAgreementStatus == AgreementStatus.NotAgreed)
                return RequestStatus.ReadyForReview;

            if (lastAction == LastAction.Approve)
                return RequestStatus.ReadyForApproval;

            return RequestStatus.None;
        }
    }
}