using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts")]
    [ApiController]
    [Authorize]
    public class CohortController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper<CreateCohortRequest, AddCohortCommand> _addCohortMapper;
        private readonly IMapper<CreateCohortWithOtherPartyRequest, AddCohortWithOtherPartyCommand> _addCohortWithOtherPartyMapper;

        public CohortController(
            IMediator mediator,
            IMapper<CreateCohortRequest, AddCohortCommand> addCohortMapper,
            IMapper<CreateCohortWithOtherPartyRequest, AddCohortWithOtherPartyCommand> addCohortWithOtherPartyMapper)
        {
            _mediator = mediator;
            _addCohortMapper = addCohortMapper;
            _addCohortWithOtherPartyMapper = addCohortWithOtherPartyMapper;
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
                LegalEntityName = result.LegalEntityName,
                ProviderName = result.ProviderName,
                IsFundedByTransfer = result.IsFundedByTransfer,
                WithParty = result.WithParty
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCohort([FromBody]CreateCohortRequest request)
        {
            var command = await _addCohortMapper.Map(request);
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCohortWithOtherParty([FromBody]CreateCohortWithOtherPartyRequest request)
        {
            var command = await _addCohortWithOtherPartyMapper.Map(request);
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }
    }
}