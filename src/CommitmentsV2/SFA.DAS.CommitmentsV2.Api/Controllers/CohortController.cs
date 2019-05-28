using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
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

        [HttpGet]
        [Route("{cohortId}")]
        public async Task<IActionResult> GetCohort(long cohortId)
        {
            var result = await _mediator.Send(new GetCohortSummaryRequest{CohortId = cohortId});

            if (result == null)
            {
                return NotFound();
            }

            return Ok(new GetCohortResponse
            {
                CohortId = result.CohortId,
                LegalEntityName = result.LegalEntityName
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCohort([FromBody]CreateCohortRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.CreateErrorResponse());
            }

            var command = await _addCohortMapper.Map(request);
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }
    }
}