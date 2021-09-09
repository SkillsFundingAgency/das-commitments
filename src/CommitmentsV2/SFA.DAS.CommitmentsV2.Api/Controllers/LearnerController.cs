using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
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

            var settings = new JsonSerializerSettings
            {
                 DateFormatString = "yyyy-MM-dd'T'HH:mm:ss"
            };

            var jsonResult = new JsonResult(new GetAllLearnersResponse()
            {
                Learners = result.Learners,
                BatchNumber = result.BatchNumber,
                BatchSize = result.BatchSize,
                TotalNumberOfBatches = result.TotalNumberOfBatches
            }, settings);
            jsonResult.StatusCode = (int)HttpStatusCode.OK;
            jsonResult.ContentType = "application/json";

            return jsonResult;
        }
    }
}
