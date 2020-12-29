using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
using SFA.DAS.Commitments.Application.Queries.GetAllStandardTrainingProgrammes;
using SFA.DAS.Commitments.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.Commitments.Application.Queries.GetTrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/trainingprogramme")]
    public class TrainingProgrammeController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public TrainingProgrammeController (IMediator mediator, ICommitmentsLogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet]
        [Route("all")]
        public async Task<IHttpActionResult> GetAll()
        {
            try
            {
                var result = await _mediator.SendAsync(new GetAllTrainingProgrammesQuery());
                return Ok(new GetAllTrainingProgrammesResponse
                {
                    TrainingProgrammes = result.TrainingProgrammes
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error getting all courses");
                return BadRequest();
            }
        }
        
        [HttpGet]
        [Route("standards")]
        public async Task<IHttpActionResult> GetAllStandards()
        {
            try
            {
                var result = await _mediator.SendAsync(new GetAllStandardTrainingProgrammesQuery());
                return Ok(new GetAllTrainingProgrammeStandardsResponse
                {
                    TrainingProgrammes = result.TrainingProgrammes
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error getting all standards");
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IHttpActionResult>  GetTrainingProgramme(string id)
        {
            try
            {
                var result = await _mediator.SendAsync(new GetTrainingProgrammeQuery
                {
                    Id = id
                });
                return Ok(new GetTrainingProgrammeResponse
                {
                    TrainingProgramme = result.TrainingProgramme
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error getting training programme {id}");
                return BadRequest();
            }
        }
    }
}