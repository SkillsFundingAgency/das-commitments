using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Tasks.Application.Queries.GetTasks;

namespace SFA.DAS.Tasks.Api.Controllers
{
    [RoutePrefix("api/tasks")]
    public class TasksController : ApiController
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IHttpActionResult> GetAll()
        {
            var response = await _mediator.SendAsync(new GetTasksRequest {Assignee = "TODO"});

            return Ok(response.Data);
        }
    }
}
