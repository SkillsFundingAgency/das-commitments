using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.BulkUpload;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IBulkUploadRepository
    {
        Task<long> InsertBulkUploadFile(string file, string fileName, long commitmentId);

        Task<BulkUploadResult> GetBulkUploadFile(long bulkUploadId);
    }
}