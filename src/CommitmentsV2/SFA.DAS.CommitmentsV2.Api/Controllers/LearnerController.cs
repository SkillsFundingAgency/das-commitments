using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Route("api/learners")]
    public class LearnerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LearnerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLearners(DateTime? sinceTime = null, int batch_number = 1, int batch_size = 1000)
        {
            var result = await _mediator.Send(new GetAllLearnersQuery(sinceTime, batch_number, batch_size));

            return Ok(new GetAllLearnersResponse()
            {
                Learners = result.Learners,
                BatchNumber = result.BatchNumber,
                BatchSize = result.BatchSize,
                TotalNumberOfBatches = result.TotalNumberOfBatches
            });
        }
    }
}
