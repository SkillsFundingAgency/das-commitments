using System.Data;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data.Transactions
{
    public interface IApprenticeshipTransactions
    {
        DynamicParameters GetApprenticeshipUpdateCreateParameters(Apprenticeship_new apprenticeship);

        Task<int> UpdateApprenticeship(IDbConnection connection, IDbTransaction trans, Apprenticeship_new apprenticeship, Caller caller);

        Task UpdateCurrentPrice(IDbConnection connection, IDbTransaction trans, Apprenticeship_new apprenticeship);
    }
}