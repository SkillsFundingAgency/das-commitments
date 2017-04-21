using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventsDocumentSerivce : IPaymentEvents
    {
        public Task<IEnumerable<DataLockEventItem>> GetDataLockEvents(
            int sinceEventId = 0,
            DateTime? sinceTime = null,
            string employerAccountId = null,
            long ukprn = 0,
            int page = 1)
        {

            /* 
             * Hello world.
                For the PaymentEvents document repo:
                Would it be a good idea to use Azure Storage blob thingi? 
                We can then add/fake document easy outside the solution. 
                Hej då.
             */
            // ToDo: need a service that enable us to test when new payment events are comming in

            var nextEventId = sinceEventId + 1;

            var list = new List<DataLockEventItem> { new DataLockEventItem() };
            return Task.Run(() => list.AsEnumerable());
        }
    }
}