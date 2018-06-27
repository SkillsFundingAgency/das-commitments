using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Enums;

namespace SFA.DAS.Commitments.Support.SubSite.Services
{
    public interface ICommitmentStatusCalculator
    {
        RequestStatus GetStatus(EditStatus editStatus, int apprenticeshipCount, LastAction lastAction, Api.Types.AgreementStatus? overallAgreementStatus, long? transferSenderId, Api.Types.TransferApprovalStatus? transferApprovalStatus);
    }
}