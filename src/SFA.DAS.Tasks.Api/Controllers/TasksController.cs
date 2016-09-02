using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Tasks.Application.Commands.CompleteTask;
using SFA.DAS.Tasks.Application.Commands.CreateTask;
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

        [Route("{assignee}")]
        public async Task<IHttpActionResult> Get(string assignee)
        {
            var response = await _mediator.SendAsync(new GetTasksRequest {Assignee = assignee});

            return Ok(response.Data);
        }

        [Route("")]
        public async Task<IHttpActionResult> Post(Domain.Entities.Task task)
        {
            await _mediator.SendAsync(new CreateTaskCommand
            {
                Assignee = task.Assignee, TaskTemplateId = task.TaskTemplateId
            });

            return Ok(); //todo: should be Created/201
        }

        [Route("{id:long:min(1)}")]
        public async Task<IHttpActionResult> Put(long id, Domain.Entities.Task task)
        {
            await _mediator.SendAsync(new CompleteTaskCommand
            {
                TaskId = id,
                CompletedBy = task.CompletedBy
            });

            return Ok();
        }
    }
}
