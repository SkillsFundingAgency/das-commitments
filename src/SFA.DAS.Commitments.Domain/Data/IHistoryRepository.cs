using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IHistoryRepository
    {
        Task InsertHistory(IEnumerable<HistoryItem> historyItems);
        Task InsertHistory(IDbConnection connection, IDbTransaction transaction, IEnumerable<HistoryItem> historyItems);
    }
}
