﻿using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/learners")]
public class LearnerController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllLearners(DateTime? sinceTime = null, int batch_number = 1, int batch_size = 1000)
    {
        var result = await mediator.Send(new GetAllLearnersQuery(sinceTime, batch_number, batch_size));

        return Ok(new GetAllLearnersResponse
        {
            Learners = result.Learners,
            BatchNumber = result.BatchNumber,
            BatchSize = result.BatchSize,
            TotalNumberOfBatches = result.TotalNumberOfBatches
        });
    }
}