using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Tasks.Application.Commands.CreateTaskAlert;
using SFA.DAS.Tasks.Application.Queries.GetTaskAlerts;
using SFA.DAS.Tasks.Domain.Entities;

namespace SFA.DAS.Tasks.Api.Controllers
{
    [RoutePrefix("api/taskalerts")]
    public class TaskAlertsController : ApiController
    {
        private readonly IMediator _mediator;

        public TaskAlertsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("{userId}", Name = "GetAllTaskAlerts")]
        public async Task<IHttpActionResult> Get(string userId)
        {
            var response = await _mediator.SendAsync(new GetTaskAlertsRequest {UserId = userId});

            return Ok(response.Data);
        }

        [Route("{userId}")]
        public async Task<IHttpActionResult> Post(string userId, TaskAlert taskAlert)
        {
            await _mediator.SendAsync(new CreateTaskAlertCommand
            {
                UserId = userId,
                TaskId = taskAlert.TaskId
            });

            // 201 for list of user's alerts (as no need for a specific route to a single alert)
            return CreatedAtRoute("GetAllTaskAlerts", new {userId}, default(TaskAlert));
        }
    }
}
