using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Application.Services;
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

        private readonly RetryService _retryService;

        public PaymentEventsService(
            IPaymentsEventsApiClient paymentsEventsApi,
            IPaymentEventMapper mapper,
            ILog logger,
            RetryService retryService)
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
            _retryService = retryService;
        }

        public async Task<IEnumerable<DataLockStatus>> GetDataLockEvents(
            long sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {
            //todo: remove cast to int once package fixed

            var result = await  _retryService.Retry(
                3, 
                () => _paymentsEventsApi.GetDataLockEvents((int)sinceEventId, sinceTime, employerAccountId, ukprn, page)
            );
            // ToDo: Do we need to thow exception if GetDataLockEvents fails? 

            return 
                result?.Items.Select(_mapper.Map) 
                ?? new DataLockStatus[0];
        }
    }
}