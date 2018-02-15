using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using Polly;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventsService : IPaymentEvents
    {
        private readonly IPaymentsEventsApiClient _paymentsEventsApi;
        private readonly IPaymentEventMapper _mapper;
        private readonly ILog _logger;
        private readonly Policy _retryPolicy;

        public PaymentEventsService(
            IPaymentsEventsApiClient paymentsEventsApi,
            IPaymentEventMapper mapper,
            ILog logger)
        {
            if(paymentsEventsApi == null)
                throw new ArgumentNullException(nameof(paymentsEventsApi));
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _paymentsEventsApi = paymentsEventsApi;
            _mapper = mapper;
            _logger = logger;
            _retryPolicy =  Policy
                .Handle<Exception>()
                .RetryAsync(3,
                    (exception, retryCount) =>
                    {
                        _logger.Warn($"Error connecting to Payment Event Api: ({exception.Message}). Retrying...attempt {retryCount})");
                    }
                );
        }

        public async Task<IEnumerable<DataLockStatus>> GetDataLockEvents(
            long sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {
            var result = await _retryPolicy.ExecuteAsync(() => _paymentsEventsApi.GetDataLockEvents(sinceEventId, sinceTime, employerAccountId, ukprn, page));

            return 
                result?.Items.Select(_mapper.Map) 
                ?? new DataLockStatus[0];
        }

        public async Task<PageOfResults<SubmissionEvent>> GetSubmissionEvents(long sinceEventId = 0, DateTime? sinceTime = null, long ukprn = 0, int page = 1)
        {
            return await _retryPolicy.ExecuteAsync(() => _paymentsEventsApi.GetSubmissionEvents(sinceEventId, sinceTime, ukprn, page));
        }
    }
}