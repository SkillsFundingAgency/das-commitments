using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.AzureStorage
{
    public class AzureBlobStorage : IAzureBlobStorage
    {
        public string StorageConnectionString { get; }
        private readonly ILog _logger;

        public AzureBlobStorage(string storageConnectionString, ILog logger)
        {
            StorageConnectionString = storageConnectionString;
            _logger = logger;
        }

        public async Task<string> ReadBlob(string containerName, string blobName)
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            if (!container.Exists())
            {
                _logger.Warn($"Container '{containerName}' not found.");
                return string.Empty;
            }

            var blob = container.GetBlockBlobReference(blobName);
            if (!blob.Exists())
                return string.Empty;

            return await blob.DownloadTextAsync();
        }

        //private async Task WriteToBlob(string containerName, string blobName, string contents)
        //{
        //    var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
        //    var blobClient = storageAccount.CreateCloudBlobClient();

        //    var container = blobClient.GetContainerReference(containerName);
        //    container.CreateIfNotExists();

        //    var blob = container.GetBlockBlobReference(blobName);

        //    //var options = new BlobRequestOptions { ServerTimeout = TimeSpan.FromMinutes(2) };

        //    using (var stream = new MemoryStream(Encoding.Default.GetBytes(contents), false))
        //    {
        //        await blob.UploadFromStreamAsync(stream);
        //    }
        //}
    }
}
