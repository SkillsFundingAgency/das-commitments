using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IBulkUploadRepository
    {
        Task<long> InsertBulkUploadFile(string file, string fileName, long commitmentId);

        Task<string> GetBulkUploadFile(long bulkUploadId);
    }
}