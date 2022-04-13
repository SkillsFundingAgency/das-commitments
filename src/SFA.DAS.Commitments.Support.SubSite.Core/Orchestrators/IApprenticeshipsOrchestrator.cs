using SFA.DAS.Commitments.Support.SubSite.Core.Models;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Support.SubSite.Core.Orchestrators
{
    public interface IApprenticeshipsOrchestrator
    {
        Task<UlnSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery);

        Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId);

        Task<CommitmentSummaryViewModel> GetCommitmentSummary(ApprenticeshipSearchQuery searchQuery);

        Task<CommitmentDetailViewModel> GetCommitmentDetails(string hashCommitmentId);
    }
}