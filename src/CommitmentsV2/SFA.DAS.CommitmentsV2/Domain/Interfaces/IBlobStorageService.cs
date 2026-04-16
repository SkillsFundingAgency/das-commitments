namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IBlobStorageService
{
    Task EnsureContainerExistsAsync(string containerName);
    Task<IReadOnlyCollection<string>> ListBlobsAsync(string containerName, string prefix = null);
    Task<BinaryData> GetBlobAsync(string containerName, string blobName);
    Task UploadAsync(string containerName, string blobName, BinaryData binaryData);
    Task DeleteAsync(string containerName, string blobName);
}
