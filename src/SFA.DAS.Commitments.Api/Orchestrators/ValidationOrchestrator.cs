using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ValidationOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public ValidationOrchestrator(IMediator mediator, ICommitmentsLogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> ValidateOverlappingApprenticeships(
            IEnumerable<ApprenticeshipOverlapValidationRequest> apprenticeshipOverlapValidationRequests)
        {
            var requests = apprenticeshipOverlapValidationRequests.ToList();

            var command = new Application.Queries.GetOverlappingApprenticeships.GetOverlappingApprenticeshipsRequest
                {
                    OverlappingApprenticeshipRequests = requests.ToList()
                };

            var response = await _mediator.SendAsync(command);

            var result = new List<ApprenticeshipOverlapValidationResult>();

            var requestGroups = response.Data.GroupBy(x => x.RequestApprenticeshipId).ToList();

            foreach (var group in requestGroups)
            {
                result.Add(new ApprenticeshipOverlapValidationResult
                {
                    Self = requests.Single(x=> x.ApprenticeshipId == group.Key),
                    OverlappingApprenticeships = response.Data.Where(x=> x.RequestApprenticeshipId == group.Key)
                });
            }

            return result;
        }
    }
}