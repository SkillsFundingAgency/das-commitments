using System.Data;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities.DataLock;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data.Transactions
{
    public interface IDataLockTransactions
    {
        Task<long> UpdateDataLockTriageStatus(IDbConnection connection, IDbTransaction trans, long dataLockEventId, TriageStatus triageStatus);

        Task<long> ResolveDataLock(IDbConnection connection, IDbTransaction trans, long dataLockEventId);
    }
}
