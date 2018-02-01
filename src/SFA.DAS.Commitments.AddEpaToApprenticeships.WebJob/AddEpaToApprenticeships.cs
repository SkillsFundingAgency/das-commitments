using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob
{
    public class AddEpaToApprenticeships : IAddEpaToApprenticeships
    {
        private readonly ILog _logger;

        private readonly IPaymentEvents _paymentEventsSerivce;

        public AddEpaToApprenticeships(ILog logger,
            IPaymentEvents paymentEventsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILog));
            _paymentEventsSerivce = paymentEventsService ?? throw new ArgumentNullException(nameof(IPaymentEvents));
        }

        public async Task Update()
        {
            long lastId = 0;

            var page = await _paymentEventsSerivce.GetSubmissionEvents(lastId);
        }
    }
}
