
using SFA.DAS.CommitmentsV2.Types;
using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Services;

public enum RequestSupportStatus
{
    None, // No use here.

    [Description("New request")]
    NewRequest,

    [Description("Sent to provider")]
    SentToProvider,

    [Description("Sent for review")]
    SentForReview,

    [Description("Ready for review")]
    ReadyForReview,

    [Description("With Provider for approval")]
    WithProviderForApproval,

    [Description("Ready for approval")]
    ReadyForApproval,

    [Description("Approved")]
    Approved,

    //With sender for approval
    [Description("Pending")]
    WithSenderForApproval,

    //Rejected by transfer
    [Description("Rejected")]
    RejectedBySender
}

public interface ICohortSupportStatusCalculator
{
    RequestSupportStatus GetStatus(EditStatus editStatus, bool hasApprenticeships, LastAction lastAction, Party party, long? transferSenderId, TransferApprovalStatus? transferApprovalStatus);
}

public sealed class CohortSupportStatusCalculator : ICohortSupportStatusCalculator
{
    public RequestSupportStatus GetStatus(EditStatus editStatus, bool hasApprenticeships, LastAction lastAction, Party party, long? transferSenderId, TransferApprovalStatus? transferApprovalStatus)
    {
        var overallAgreementStatus = GetAgreementStatus(party);

        if (transferSenderId.HasValue)
        {
            return GetTransferStatus(editStatus, transferApprovalStatus, lastAction, hasApprenticeships, overallAgreementStatus);
        }

        if (editStatus == EditStatus.Both)
            return RequestSupportStatus.Approved;

        if (editStatus == EditStatus.ProviderOnly)
            return GetProviderOnlyStatus(lastAction, hasApprenticeships);

        if (editStatus == EditStatus.EmployerOnly)
            return GetEmployerOnlyStatus(lastAction, hasApprenticeships, overallAgreementStatus);

        return RequestSupportStatus.None;
    }

    private RequestSupportStatus GetProviderOnlyStatus(LastAction lastAction, bool hasApprenticeships)
    {
        if (!hasApprenticeships || lastAction == LastAction.None)
            return RequestSupportStatus.SentToProvider;

        if (lastAction == LastAction.Amend)
            return RequestSupportStatus.SentForReview;

        if (lastAction == LastAction.Approve)
            return RequestSupportStatus.WithProviderForApproval;

        return RequestSupportStatus.None;
    }

    private RequestSupportStatus GetEmployerOnlyStatus(LastAction lastAction, bool hasApprenticeships, AgreementStatus? overallAgreementStatus)
    {
        if (!hasApprenticeships || lastAction == LastAction.None || lastAction == LastAction.AmendAfterRejected)
            return RequestSupportStatus.NewRequest;

        // LastAction.Approve > LastAction.Amend, but then AgreementStatus >= ProviderAgreed, so no need for > on LastAction??
        if (lastAction >= LastAction.Amend && overallAgreementStatus == AgreementStatus.NotAgreed)
            return RequestSupportStatus.ReadyForReview;

        if (lastAction == LastAction.Approve)
            return RequestSupportStatus.ReadyForApproval;

        return RequestSupportStatus.None;
    }

    private RequestSupportStatus GetTransferStatus(EditStatus edit, TransferApprovalStatus? transferApproval, LastAction lastAction, bool hasApprenticeships, AgreementStatus? overallAgreementStatus)
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
                            return RequestSupportStatus.WithSenderForApproval;

                        case EditStatus.EmployerOnly:
                            //todo: need to set to draft after rejected by sender and edited by receiver (but not sent to provider)
                            return GetEmployerOnlyStatus(lastAction, hasApprenticeships, overallAgreementStatus);

                        case EditStatus.ProviderOnly:
                            return GetProviderOnlyStatus(lastAction, hasApprenticeships);

                        default:
                            throw new Exception("Unexpected EditStatus");
                    }
                }

            case TransferApprovalStatus.Approved:
                if (edit != EditStatus.Both)
                    throw new Exception($"{invalidStateExceptionMessagePrefix}If approved by sender, must be approved by receiver and provider");
                return RequestSupportStatus.None;

            case TransferApprovalStatus.Rejected:
                if (edit != EditStatus.EmployerOnly)
                    throw new Exception($"{invalidStateExceptionMessagePrefix}If just rejected by sender, must be with receiver");
                return RequestSupportStatus.RejectedBySender;

            default:
                throw new Exception("Unexpected TransferApprovalStatus");
        }
    }

    private static AgreementStatus GetAgreementStatus(Party party)
    {
        if (party == Party.None)
        {
            return AgreementStatus.NotAgreed;
        }

        if (party == Party.Employer)
        {
            return AgreementStatus.EmployerAgreed;
        }

        if (party == Party.Provider)
        {
            return AgreementStatus.ProviderAgreed;
        }

        if (party.HasFlag(Party.Provider) && party.HasFlag(Party.Employer))
        {
            return AgreementStatus.BothAgreed;
        }

        return AgreementStatus.NotAgreed;
    }
}
