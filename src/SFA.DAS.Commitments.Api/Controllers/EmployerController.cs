using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/employer")]
    public class EmployerController : ApiController
    {
        private readonly IMediator _mediator;

        public EmployerController(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        [Route("{id}/commitments")]
        public async Task<IHttpActionResult> GetCommitments(long id)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetEmployerCommitmentsRequest { AccountId = id });

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}
