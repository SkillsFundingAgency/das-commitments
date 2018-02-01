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
        private readonly IAssessmentOrgs _assessmentOrgsService;
        private readonly IPaymentEvents _paymentEventsService;

        public AddEpaToApprenticeships(ILog logger,
            IAssessmentOrgs assessmentOrgsService,
            IPaymentEvents paymentEventsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILog));
            _assessmentOrgsService = assessmentOrgsService ?? throw new ArgumentNullException(nameof(assessmentOrgsService));
            _paymentEventsService = paymentEventsService ?? throw new ArgumentNullException(nameof(IPaymentEvents));
        }

        public async Task Update()
        {
            var organisationSummaries = await _assessmentOrgsService.AllAsync();

            long lastId = 0;

            var page = await _paymentEventsService.GetSubmissionEvents(lastId);
        }
    }
}
