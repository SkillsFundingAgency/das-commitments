using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Infrastructure.AzureStorage
{
    // shouldn't really be in the same assembly as the concrete, we should use the stairway pattern, but we fit in with what's currently here
    public interface IAzureBlobStorage
    {
        string StorageConnectionString { get; }

        Task<string> ReadBlob(string containerName, string blobName);
    }
}
