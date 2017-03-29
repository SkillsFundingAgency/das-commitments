using System.Data;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public interface IApprenticeshipUpdateTransactions
    {
        Task<long> CreateApprenticeshipUpdate(IDbConnection connection, IDbTransaction trans, ApprenticeshipUpdate apprenticeshipUpdate);

        Task<long> UpdateApprenticeshipReferenceAndUln(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship);
    }
}
