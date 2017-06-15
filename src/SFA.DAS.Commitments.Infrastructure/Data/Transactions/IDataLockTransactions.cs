using System.Data;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public interface IDataLockTransactions
    {
        Task<long> UpdateDataLockTriageStatus(IDbConnection connection, IDbTransaction trans, long dataLockEventId, TriageStatus triageStatus);

        Task<long> ResolveDataLock(IDbConnection connection, IDbTransaction trans, long dataLockEventId);
    }
}
