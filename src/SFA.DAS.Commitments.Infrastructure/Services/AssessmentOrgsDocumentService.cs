using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Api.Types.AssessmentOrgs;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class AssessmentOrgsDocumentService : IAssessmentOrgs
    {
        private readonly string _storageConnectionString;

        private readonly ILog _logger;

        public AssessmentOrgsDocumentService(string storageConnectionString, ILog logger)
        {
            _storageConnectionString = storageConnectionString;
            _logger = logger;
        }

        public async Task<IEnumerable<OrganisationSummary>> AllAsync()
        {
            const string containerName = "assessmentorgs-repository";
            const string blobName = "assessment_orgs.json";

            var result = await ReadFromStorageAsync(containerName, blobName);
            if (string.IsNullOrEmpty(result))
                return new OrganisationSummary[0];

            return JsonConvert.DeserializeObject<IEnumerable<OrganisationSummary>>(result);
        }

        //todo: don't just cut and paste, put in a static util class or create a base class
        private async Task<string> ReadFromStorageAsync(string containerName, string blobName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
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

        //private async Task WriteTestDataToStorageAsync()
        //{
        //}

        //private async Task WriteToStorageAsync(string containerName, string blobName, string contents)
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
