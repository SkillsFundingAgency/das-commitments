using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using System;


namespace SFA.DAS.Commitments.Support.SubSite.Services
{
    public sealed class CommitmentStatusCalculator : ICommitmentStatusCalculator
    {
        public RequestStatus GetStatus(EditStatus editStatus, int apprenticeshipCount, LastAction lastAction, AgreementStatus? overallAgreementStatus, long? transferSenderId, TransferApprovalStatus? transferApprovalStatus)
        {
            bool hasApprenticeships = apprenticeshipCount > 0;

            if (transferSenderId.HasValue)
            {
                return GetTransferStatus(editStatus, transferApprovalStatus, lastAction, hasApprenticeships, overallAgreementStatus);
            }

            if (editStatus == EditStatus.Both)
                return RequestStatus.Approved;

            if (editStatus == EditStatus.ProviderOnly)
                return GetProviderOnlyStatus(lastAction, hasApprenticeships);

            if (editStatus == EditStatus.EmployerOnly)
                return GetEmployerOnlyStatus(lastAction, hasApprenticeships, overallAgreementStatus);

            return RequestStatus.None;
        }

        private  RequestStatus GetProviderOnlyStatus(LastAction lastAction, bool hasApprenticeships)
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
            if (!hasApprenticeships || lastAction == LastAction.None || lastAction == LastAction.AmendAfterRejected)
                return RequestStatus.NewRequest;

            // LastAction.Approve > LastAction.Amend, but then AgreementStatus >= ProviderAgreed, so no need for > on LastAction??
            if (lastAction >= LastAction.Amend && overallAgreementStatus == AgreementStatus.NotAgreed)
                return RequestStatus.ReadyForReview;

            if (lastAction == LastAction.Approve)
                return RequestStatus.ReadyForApproval;

            return RequestStatus.None;
        }

        private RequestStatus GetTransferStatus(EditStatus edit, TransferApprovalStatus? transferApproval, LastAction lastAction, bool hasApprenticeships, AgreementStatus? overallAgreementStatus)
        {
            const string invalidStateExceptionMessagePrefix = "Transfer funder commitment in invalid state: ";

            if (edit >= EditStatus.Neither)
                throw new Exception("Unexpected EditStatus");

            switch (transferApproval ?? TransferApprovalStatus.Pending)
            {
                case TransferApprovalStatus.Pending:
                    {
                        switch (edit)
                        {
                            case EditStatus.Both:
                                return RequestStatus.WithSenderForApproval;
                            case EditStatus.EmployerOnly:
                                //todo: need to set to draft after rejected by sender and edited by receiver (but not sent to provider)
                                return GetEmployerOnlyStatus(lastAction, hasApprenticeships, overallAgreementStatus);
                            case EditStatus.ProviderOnly:
                                return GetProviderOnlyStatus(lastAction, hasApprenticeships);
                            default:
                                throw new Exception("Unexpected EditStatus");
                        }
                    }

                case TransferApprovalStatus.TransferApproved:
                    if (edit != EditStatus.Both)
                        throw new Exception($"{invalidStateExceptionMessagePrefix}If approved by sender, must be approved by receiver and provider");
                    return RequestStatus.None;

                case TransferApprovalStatus.TransferRejected:
                    if (edit != EditStatus.EmployerOnly)
                        throw new Exception($"{invalidStateExceptionMessagePrefix}If just rejected by sender, must be with receiver");
                    return RequestStatus.RejectedBySender;

                default:
                    throw new Exception("Unexpected TransferApprovalStatus");
            }
        }
    }
}