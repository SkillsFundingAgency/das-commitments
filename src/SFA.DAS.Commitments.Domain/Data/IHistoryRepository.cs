using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IHistoryRepository
    {
        Task CreateCommitmentHistory(CommitmentHistoryDbItem item);

        Task CreateApprenticeship(ApprenticeshipHistoryDbItem item);
    }
}