using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IHistoryRepository
    {
        Task CreateCommitmentHistory(CommitmentHistoryItem item);

        Task CreateApprenticeship(ApprenticeshipHistoryItem item);
    }
}