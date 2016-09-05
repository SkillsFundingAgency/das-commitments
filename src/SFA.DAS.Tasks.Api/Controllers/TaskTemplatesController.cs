using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Tasks.Application.Commands.CreateTaskTemplate;
using SFA.DAS.Tasks.Application.Queries.GetTaskTemplates;
using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.Tasks.Api.Controllers
{
    [RoutePrefix("api/tasktemplates")]
    public class TaskTemplatesController : ApiController
    {
        private readonly IMediator _mediator;

        public TaskTemplatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("", Name = "GetAllTaskTemplates")]
        public async Task<IHttpActionResult> Get()
        {
            var response = await _mediator.SendAsync(new GetTaskTemplatesRequest());

            return Ok(response.Data);
        }

        [Route("")]
        public async Task<IHttpActionResult> Post(TaskTemplate taskTemplate)
        {
            await _mediator.SendAsync(new CreateTaskTemplateCommand
            {
                Name = taskTemplate.Name
            });

            // 201 for list of all task templates (as no need for a specific route to a single template)
            return CreatedAtRoute("GetAllTaskTemplates", new { }, default(TaskTemplate));
        }
    }
}
