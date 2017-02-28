using System.Data;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public interface IApprenticeshipTransactions
    {
        Task<long> CreateApprenticeship(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship);

        DynamicParameters GetApprenticeshipUpdateCreateParameters(Apprenticeship apprenticeship);
    }
}