using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Support.SubSite.Services
{
    public interface ICommitmentStatusCalculator
    {
        RequestStatus GetStatus(EditStatus editStatus, int apprenticeshipCount, LastAction lastAction, AgreementStatus? overallAgreementStatus, long? transferSenderId, TransferApprovalStatus? transferApprovalStatus);
    }
}