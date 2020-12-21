using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TrainingProgrammeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TrainingProgrammeController> _logger;

        public TrainingProgrammeController (IMediator mediator, ILogger<TrainingProgrammeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _mediator.Send(new GetAllTrainingProgrammesQuery());
                return Ok(new GetAllTrainingProgrammesResponse
                {
                    TrainingProgrammes = result.TrainingProgrammes
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting all courses");
                return BadRequest();
            }
        }
        
        [HttpGet]
        [Route("standards")]
        public async Task<IActionResult> GetAllStandards()
        {
            try
            {
                var result = await _mediator.Send(new GetAllTrainingProgrammeStandardsQuery());
                return Ok(new GetAllTrainingProgrammeStandardsResponse
                {
                    TrainingProgrammes = result.TrainingProgrammes
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting all standards");
                return BadRequest();
            }
        }
    }
}