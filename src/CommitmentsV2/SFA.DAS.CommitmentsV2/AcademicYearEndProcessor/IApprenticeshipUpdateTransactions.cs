using System.Data;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data.Transactions
{
    public interface IApprenticeshipUpdateTransactions
    {
        Task<long> CreateApprenticeshipUpdate(IDbConnection connection, IDbTransaction trans, ApprenticeshipUpdate_new apprenticeshipUpdate);

        Task<long> UpdateApprenticeshipReferenceAndUln(IDbConnection connection, IDbTransaction trans, Apprenticeship_new apprenticeship);
    }
}
