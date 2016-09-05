using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Tasks.Application.Commands.CompleteTask;
using SFA.DAS.Tasks.Application.Commands.CreateTask;
using SFA.DAS.Tasks.Application.Queries.GetTasks;
using Task = SFA.DAS.Tasks.Domain.Entities.Task;

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

        [Route("{assignee}", Name = "GetAllTasks")]
        public async Task<IHttpActionResult> Get(string assignee)
        {
            var response = await _mediator.SendAsync(new GetTasksRequest {Assignee = assignee});

            return Ok(response.Data);
        }

        [Route("{assignee}")]
        public async Task<IHttpActionResult> Post(string assignee, Task task)
        {
            await _mediator.SendAsync(new CreateTaskCommand
            {
                Assignee = assignee, TaskTemplateId = task.TaskTemplateId
            });

            // 201 for list of assignee's tasks (as no need for a specific route to a single task)
            return CreatedAtRoute("GetAllTasks", new {assignee}, default(Task));
        }

        [Route("{id:long:min(1)}")]
        public async Task<IHttpActionResult> Put(long id, Task task)
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
