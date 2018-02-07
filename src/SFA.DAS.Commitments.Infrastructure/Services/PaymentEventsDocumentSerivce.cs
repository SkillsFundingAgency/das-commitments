using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventsDocumentSerivce : IPaymentEvents
    {
        private readonly string _storageConnectionString;

        private readonly IPaymentEventMapper _mapper;

        private readonly ILog _logger;

        public PaymentEventsDocumentSerivce(
            string storageConnectionString, 
            IPaymentEventMapper mapper,
            ILog logger)
        {
            // ToDo: Move azure storage to separate file?
            _storageConnectionString = storageConnectionString;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DataLockStatus>> GetDataLockEvents(
            long sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {
            var result = await GetDataAsync(sinceEventId + 1);
            return
                result?.Items.Select(_mapper.Map)
                ?? new DataLockStatus[0];
        }

        public async Task<PageOfResults<SubmissionEvent>> GetSubmissionEventsAsync(long sinceEventId = 0,
            DateTime? sinceTime = null, long ukprn = 0, int page = 1)
        {
            const string containerName = "paymentevents-repository";
            var fileName = $"{sinceEventId+1}_submission_event.json";
            var result = await ReadFromStorageAsync(containerName, fileName);
            if (string.IsNullOrEmpty(result))
                return new PageOfResults<SubmissionEvent> {PageNumber = 1, TotalNumberOfPages = 1, Items = new SubmissionEvent[0]};

            return JsonConvert.DeserializeObject<PageOfResults<SubmissionEvent>>(result);
        }

        private async Task<PageOfResults<DataLockEvent>> GetDataAsync(long nextEventId)
        {
            const string containerName = "paymentevents-repository";
            var fileName = $"{nextEventId}_payment_event.json";
            var result = await ReadFromStorageAsync(containerName, fileName);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<PageOfResults<DataLockEvent>>(result);
        }

        //private string ReadFromStoage(string fileName)
        //{
        //    const string ContainerName = "paymentevents-repository";
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
        //    string text = string.Empty;

        //    if (!container.Exists())
        //    {
        //        _logger.Warn($"File repository not found. Container Name {ContainerName}");
        //        return text;
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        CloudBlockBlob file = container.GetBlockBlobReference(fileName);

        //        if (!file.Exists())
        //        {
        //            return text;
        //        }

        //        file.DownloadToStream(memoryStream);
        //        text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        //    }
        //    return text;
        //}

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
    }
}