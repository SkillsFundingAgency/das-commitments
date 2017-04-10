using System.Data;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public interface IHistoryTransactions
    {
        Task CreateCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem);
        Task DeleteCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem);
        Task UpdateCommitment(IDbConnection connection, IDbTransaction trans, CommitmentChangeType changeType, CommitmentHistoryItem commitmentHistoryItem);
        Task AddApprenticeshipForCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem);
        Task DeleteApprenticeshipForCommitment(IDbConnection connection, IDbTransaction transactions, CommitmentHistoryItem apprenticeshipHistoryItem);
        Task UpdateApprenticeshipForCommitment(IDbConnection connection, IDbTransaction trans, CommitmentHistoryItem commitmentHistoryItem);

        Task CreateApprenticeship(IDbConnection connection, IDbTransaction trans, ApprenticeshipHistoryItem apprenticeshipHistoryItem);
        Task UpdateApprenticeship(IDbConnection connection, IDbTransaction trans, ApprenticeshipHistoryItem apprenticeshipHistoryItem);

        Task UpdateApprenticeshipStatus(IDbConnection connection, IDbTransaction trans, PaymentStatus newStatus, ApprenticeshipHistoryItem apprenticeshipHistoryItem);
    }
}