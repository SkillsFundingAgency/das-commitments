using System.Data;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public interface IHistoryTransactions
    {
        Task UpdateApprenticeshipStatus(IDbConnection connection, IDbTransaction trans, PaymentStatus newStatus, ApprenticeshipHistoryItem apprenticeshipHistoryItem);
    }
}