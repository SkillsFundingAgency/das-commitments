using SFA.DAS.Commitments.Support.SubSite.Models;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public interface IApprenticeshipsOrchestrator
    {
        Task<UlnSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery);

        Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId);

        Task<CommitmentSummaryViewModel> GetCommitmentSummary(ApprenticeshipSearchQuery searchQuery);

        Task<CommitmentDetailViewModel> GetCommitmentDetails(string hashCommitmentId, string accountHashedId);
    }
}