using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts")]
    public class CohortController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper<CreateCohortRequest, AddCohortCommand> _addCohortMapper;

        public CohortController(
            IMediator mediator,
            IMapper<CreateCohortRequest, AddCohortCommand> addCohortMapper)
        {
            _mediator = mediator;
            _addCohortMapper = addCohortMapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCohort([FromBody]CreateCohortRequest request)
        {
            var result = await _mediator.Send(_addCohortMapper.Map(request));

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }
    }
}