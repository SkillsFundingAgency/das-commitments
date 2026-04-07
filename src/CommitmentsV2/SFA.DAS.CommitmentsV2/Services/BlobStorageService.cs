using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class BlobStorageService(
    CommitmentsV2Configuration configuration,
    ILogger<BlobStorageService> logger)
    : IBlobStorageService
{
    private readonly object _blobServiceClientLock = new();
    private readonly SemaphoreSlim _knownContainersSemaphoreSlim = new(1, 1);
    private readonly HashSet<string> _knownContainers = [];
    private BlobServiceClient _blobServiceClient;

    public async Task EnsureContainerExistsAsync(string containerName)
    {
        logger.LogInformation("Beginning check {Container} exists", containerName);
        var container = await GetContainerClient(containerName, createContainerIfMissing: true);
        if (container is null)
        {
            return;
        }
        logger.LogInformation("Completed check {Container} exists", containerName);
    }

    public async Task<IReadOnlyCollection<string>> ListBlobsAsync(string containerName, string prefix = null)
    {
        logger.LogInformation("Beginning listing blobs in {Container}", containerName);
        var container = await GetContainerClient(containerName, createContainerIfMissing: true);
        if (container is null)
        {
            return [];
        }

        var blobs = await container
            .GetBlobsAsync(prefix: prefix)
            .Select(blob => blob.Name)
            .ToListAsync();

        logger.LogInformation("Completed listing blobs in {Container}", containerName);
        return blobs;
    }

    public async Task<BinaryData> GetBlobAsync(string containerName, string blobName)
    {
        logger.LogInformation("Beginning reading {BlobName} from {Container}", blobName, containerName);
        var container = await GetContainerClient(containerName, createContainerIfMissing: true);
        if (container is null)
        {
            return BinaryData.FromString(string.Empty);
        }

        try
        {
            var blobClient = container.GetBlobClient(blobName);
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            logger.LogInformation("Completed reading {BlobName} from {Container}", blobName, containerName);
            return downloadResult.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogError(ex, "Blob {BlobName} from {Container} not found", blobName, containerName);
            throw;
        }
    }

    public async Task UploadAsync(string containerName, string blobName, BinaryData binaryData)
    {
        logger.LogInformation("Beginning upsert {BlobName} to {Container}", blobName, containerName);
        var container = await GetContainerClient(containerName, createContainerIfMissing: true);
        if (container is null)
        {
            return;
        }

        var blobClient = container.GetBlobClient(blobName);
        await blobClient.UploadAsync(binaryData, overwrite: true);
        logger.LogInformation("Completed upsert {BlobName} to {Container}", blobName, containerName);
    }

    public async Task DeleteAsync(string containerName, string blobName)
    {
        logger.LogInformation("Beginning deleting {BlobName} from {Container}", blobName, containerName);
        var container = await GetContainerClient(containerName, createContainerIfMissing: true);
        if (container is null)
        {
            return;
        }

        await container.DeleteBlobIfExistsAsync(blobName);
        logger.LogInformation("Completed deleting {BlobName} from {Container}", blobName, containerName);
    }

    private async Task<BlobContainerClient> GetContainerClient(string containerName, bool createContainerIfMissing)
    {
        if (string.IsNullOrWhiteSpace(containerName))
        {
            logger.LogWarning("Blob container name is not configured.");
            return null;
        }

        var blobServiceClient = GetBlobServiceClient();
        if (blobServiceClient is null)
        {
            return null;
        }

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (createContainerIfMissing)
        {
            await CreateContainerIfNotExistsAsync(containerClient);
        }

        return containerClient;
    }

    private BlobServiceClient GetBlobServiceClient()
    {
        if (_blobServiceClient is not null)
        {
            return _blobServiceClient;
        }

        lock (_blobServiceClientLock)
        {
            if (_blobServiceClient is not null)
            {
                return _blobServiceClient;
            }

            if (string.IsNullOrWhiteSpace(configuration.StorageConnectionString))
            {
                logger.LogWarning("StorageConnectionString is not configured.");
                return null;
            }

            _blobServiceClient = new BlobServiceClient(configuration.StorageConnectionString);
            return _blobServiceClient;
        }
    }

    private async Task CreateContainerIfNotExistsAsync(BlobContainerClient containerClient)
    {
        if (_knownContainers.Contains(containerClient.Name))
        {
            return;
        }

        await _knownContainersSemaphoreSlim.WaitAsync();
        try
        {
            if (_knownContainers.Contains(containerClient.Name))
            {
                return;
            }

            if (!await containerClient.ExistsAsync())
            {
                await containerClient.CreateAsync();
                logger.LogInformation("Created missing blob container {ContainerName}.", containerClient.Name);
            }

            _knownContainers.Add(containerClient.Name);
        }
        finally
        {
            _knownContainersSemaphoreSlim.Release();
        }
    }
}
