using System.Data;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public interface ICommitmentTransactions
    {
        Task<long> CreateRelationship(IDbConnection connection, IDbTransaction trans, Relationship relationship);
    }
}
