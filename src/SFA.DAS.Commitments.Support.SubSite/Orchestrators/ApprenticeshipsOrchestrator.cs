using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly ILog _logger;
        private readonly IMediator _mediator;

        public ApprenticeshipsOrchestrator(ILog logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<int> GetApprenticeshipsByUln(String unl)
        {
            _logger.Trace("Retrieving Apprenticeships Record Count");


            var response = await _mediator.SendAsync(new GetApprenticeshipsByUlnRequest
            {
                Uln = unl
            });

            _logger.Info($"Apprenticeships Record Count: {response.TotalCount}");

            return 0;
        }

    }
}