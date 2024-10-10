using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TrainingProgrammeController(IMediator mediator, ILogger<TrainingProgrammeController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await mediator.Send(new GetAllTrainingProgrammesQuery());
            
            return Ok(new GetAllTrainingProgrammesResponse
            {
                TrainingProgrammes = result.TrainingProgrammes
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all courses");
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("standards")]
    public async Task<IActionResult> GetAllStandards()
    {
        try
        {
            var result = await mediator.Send(new GetAllTrainingProgrammeStandardsQuery());
            return Ok(new GetAllTrainingProgrammeStandardsResponse
            {
                TrainingProgrammes = result.TrainingProgrammes
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all standards");
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetTrainingProgramme(string id)
    {
        try
        {
            var result = await mediator.Send(new GetTrainingProgrammeQuery
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
            logger.LogError(e, "Error getting training programme {id}", id);
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("{standardUId}/version")]
    public async Task<IActionResult> GetTrainingProgrammeVersion(string standardUId)
    {
        try
        {
            var result = await mediator.Send(new GetTrainingProgrammeVersionQuery(standardUId));

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
            logger.LogError(e, "Error getting standard options for {standardUId}", standardUId);
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("{courseCode}/version/{version}")]
    public async Task<IActionResult> GetTrainingProgrammeVersion(string courseCode, string version)
    {
        try
        {
            var result = await mediator.Send(new GetTrainingProgrammeVersionQuery(courseCode, version));

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
            logger.LogError(e, "Error getting standard version for standard {courseCode} version {version}", courseCode, version);
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("{id}/versions")]
    public async Task<IActionResult> GetTrainingProgrammeVersions(string id)
    {
        try
        {
            var result = await mediator.Send(new GetTrainingProgrammeVersionsQuery(id));

            if (result.TrainingProgrammes == null)
            {
                return NotFound();
            }

            return Ok(new GetTrainingProgrammeVersionsResponse
            {
                TrainingProgrammeVersions = result.TrainingProgrammes
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting standard versions for {id}", id);
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("{standardUId}/newer-versions")]
    public async Task<IActionResult> GetNewerTrainingProgrammeVersions(string standardUId)
    {
        try
        {
            var result = await mediator.Send(new GetNewerTrainingProgrammeVersionsQuery { StandardUId = standardUId });

            return Ok(new GetNewerTrainingProgrammeVersionsResponse
            {
                NewerVersions = result.NewerVersions
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting newer versions for standardUId {standardUId}", standardUId);
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("calculate-version/{courseCode:int}")]
    public async Task<IActionResult> GetCalculatedTrainingProgrammeVersion(int courseCode, [FromQuery] GetTrainingProgrammeVersionRequest request)
    {
        try
        {
            var result = await mediator.Send(new GetCalculatedTrainingProgrammeVersionQuery
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
        catch (Exception)
        {
            return BadRequest();
        }
    }
}