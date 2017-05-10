using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventsService : IPaymentEvents
    {
        private readonly IPaymentsEventsApiClient _paymentsEventsApi;

        private readonly IPaymentEventMapper _mapper;

        private readonly ILog _logger;

        public int RetryWaitTimeInSeconds { get; set; } = 3;

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
        }

        public async Task<IEnumerable<DataLockStatus>> GetDataLockEvents(
            long sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {
            //todo: remove cast to int once package fixed

            var result = await  Retry(
                3, 
                () => _paymentsEventsApi.GetDataLockEvents((int)sinceEventId, sinceTime, employerAccountId, ukprn, page)
            );
            // ToDo: Do we need to thow exception if GetDataLockEvents fails? 

            return 
                result?.Items.Select(_mapper.Map) 
                ?? new DataLockStatus[0];
        }

        private async Task<T> Retry<T>(int retryCount, Func<Task<T>> action)
        {
            var exception = new List<Exception>();
            do
            {
                --retryCount;
                try
                {
                    return await action.Invoke();
                }
                catch (Exception ex)
                {
                    exception.Add(ex);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(RetryWaitTimeInSeconds));
                }
            }
            while (retryCount > 0);

            _logger.Warn(exception.FirstOrDefault(), $"Not able to call service, tried {exception.Count} times");
            return default(T);
        }
    }
}