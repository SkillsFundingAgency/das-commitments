using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{apprenticeshipId}/datalocks")]
    public class DataLocksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public DataLocksController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetDataLocks(long apprenticeshipId)
        {
            var query = new GetDataLocksQuery(apprenticeshipId);
            var result = await _mediator.Send(query);

            var response = await _modelMapper.Map<GetDataLocksResponse>(result);
            return Ok(response);
        }
    }
}