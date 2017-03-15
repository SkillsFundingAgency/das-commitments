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

        public async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> ValidateOverlappingApprenticeships(IEnumerable<ApprenticeshipOverlapValidationRequest> requests)
        {
            var command = new Application.Queries.GetOverlappingApprenticeships.GetOverlappingApprenticeshipsRequest
                {
                    OverlappingApprenticeshipRequests = requests.ToList()
                };

            var response = await _mediator.SendAsync(command);

            var result = new List<ApprenticeshipOverlapValidationResult>();

            var ulngroups = response.Data.GroupBy(x => x.Apprenticeship.ULN);

            foreach (var group in ulngroups)
            {
                result.Add(new ApprenticeshipOverlapValidationResult
                {
                    //todo: remove single
                    Self = requests.Single(x=> x.Uln.Equals(group.Key, StringComparison.InvariantCultureIgnoreCase)),
                    OverlappingApprenticeships = response.Data.Where(x=> x.Apprenticeship.ULN.Equals(group.Key, StringComparison.InvariantCultureIgnoreCase))
                });
            }

            return result;
        }
    }
}