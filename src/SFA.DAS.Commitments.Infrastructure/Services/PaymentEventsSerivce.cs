using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventsSerivce : IPaymentEvents
    {
        private readonly IPaymentsEventsApiClient _paymentsEventsApi;

        private readonly IPaymentEventMapper _mapper;

        public PaymentEventsSerivce(
            IPaymentsEventsApiClient paymentsEventsApi,
            IPaymentEventMapper mapper)
        {
            if(paymentsEventsApi == null)
                throw new ArgumentNullException(nameof(paymentsEventsApi));
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            _paymentsEventsApi = paymentsEventsApi;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DataLockEventItem>> GetDataLockEvents(
            int sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {
            // ToDo: Retry policy?
            // ToDo: Set up config
            // ToDO: Set up what service to use
            var result = await _paymentsEventsApi.GetDataLockEvents(sinceEventId, sinceTime, employerAccountId, ukprn, page);

            return result.Items.Select(_mapper.Map);
        }
    }
}