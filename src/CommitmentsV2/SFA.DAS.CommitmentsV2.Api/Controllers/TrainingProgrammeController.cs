using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;

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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult>  GetTrainingProgramme(string id)
        {
            try
            {
                var result = await _mediator.Send(new GetTrainingProgrammeQuery
                {
                    Id = id
                });
                
                if (result.TrainingProgramme == null)
                {
                    return NotFound();
                }
                
                return Ok(new GetTrainingProgrammeResponse
                {
                    TrainingProgramme = result.TrainingProgramme
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting training programme {id}");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("calculate-version/{courseCode}")]
        public async Task<IActionResult> GetCalculatedTrainingProgrammeVersion(int courseCode, [FromQuery] GetTrainingProgrammeVersionRequest request)
        {
            try
            {
                var result = await _mediator.Send(new GetCalculatedTrainingProgrammeVersionQuery
                {
                    CourseCode = courseCode,
                    StartDate = request.StartDate.Value
                });

                if (result.TrainingProgramme == null)
                {
                    return NotFound();
                }

                return Ok(new GetTrainingProgrammeResponse
                {
                    TrainingProgramme = result.TrainingProgramme
                });
            }
            catch (Exception )
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("{standardUId}/version")]
        public async Task<IActionResult> GetTrainingProgrammeVersion(string standardUId)
        {
            try
            {
                var result = await _mediator.Send(new GetTrainingProgrammeVersionQuery(standardUId));

                if (result.TrainingProgramme == null)
                {
                    return NotFound();
                }

                return Ok(new GetTrainingProgrammeResponse 
                {
                    TrainingProgramme = result.TrainingProgramme
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting standard options for {standardUId}");
                return BadRequest();
            }
        }
    }
}