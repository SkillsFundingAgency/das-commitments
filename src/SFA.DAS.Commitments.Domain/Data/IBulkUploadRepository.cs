using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IBulkUploadRepository
    {
        Task<long> InsertBulkUploadFile(string file);

        Task<string> GetBulkUploadFile(long bulkUploadId);
    }
}