using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.AzureStorage;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventsDocumentService : IPaymentEvents
    {
        private readonly IAzureBlobStorage _azureBlobStorage;
        private readonly IPaymentEventMapper _mapper;

        public PaymentEventsDocumentService(
            IAzureBlobStorage azureBlobStorage,
            IPaymentEventMapper mapper)
        {
            _azureBlobStorage = azureBlobStorage;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DataLockStatus>> GetDataLockEvents(
            long sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {
            var result = await GetData(sinceEventId + 1);
            return
                result?.Items.Select(_mapper.Map)
                ?? new DataLockStatus[0];
        }

        public async Task<PageOfResults<SubmissionEvent>> GetSubmissionEvents(long sinceEventId = 0,
            DateTime? sinceTime = null, long ukprn = 0, int page = 1)
        {
            const string containerName = "paymentevents-repository";
            var fileName = $"{sinceEventId+1}_submission_event.json";
            var result = await _azureBlobStorage.ReadBlob(containerName, fileName);
            if (string.IsNullOrEmpty(result))
                return new PageOfResults<SubmissionEvent> {PageNumber = 1, TotalNumberOfPages = 0, Items = new SubmissionEvent[0]};

            return JsonConvert.DeserializeObject<PageOfResults<SubmissionEvent>>(result);
        }

        private async Task<PageOfResults<DataLockEvent>> GetData(long nextEventId)
        {
            const string containerName = "paymentevents-repository";
            var fileName = $"{nextEventId}_payment_event.json";
            var result = await _azureBlobStorage.ReadBlob(containerName, fileName);
            if (string.IsNullOrEmpty(result))
                return null;

            return JsonConvert.DeserializeObject<PageOfResults<DataLockEvent>>(result);
        }
    }
}