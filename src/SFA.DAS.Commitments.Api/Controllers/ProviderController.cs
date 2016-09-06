using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/provider")]
    public class ProviderController : ApiController
    {
        private readonly IMediator _mediator;

        public ProviderController(IMediator mediator)
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
                var response = await _mediator.SendAsync(new GetProviderCommitmentsRequest { ProviderId = id });

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [Route("{providerId}/commitments/{commitmentId}")]
        public async Task<IHttpActionResult> GetCommitment(long providerId, long commitmentId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetCommitmentRequest { ProviderId = providerId, CommitmentId = commitmentId });

                if (response?.Data == null)
                {
                    return NotFound();
                }

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForProvider")]

        public async Task<IHttpActionResult> GetApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetApprenticeshipRequest { ProviderId = providerId, CommitmentId = commitmentId, ApprenticeshipId = apprenticeshipId });

                if (response.Data == null)
                {
                    return NotFound();
                }

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships")]
        public async Task<IHttpActionResult> CreateApprenticeship(long providerId, long commitmentId, Apprenticeship apprenticeship)
        {
            try
            {
                var apprenticeshipId = await _mediator.SendAsync(new CreateApprenticeshipCommand { ProviderId = providerId, CommitmentId = commitmentId, Apprenticeship = apprenticeship });

                return CreatedAtRoute("GetApprenticeshipForProvider", new { providerId = providerId, commitmentId = commitmentId, apprenticeshipId = apprenticeshipId }, default(Apprenticeship));
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        public async Task<IHttpActionResult> PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipCommand { ProviderId = providerId, CommitmentId = commitmentId, ApprenticeshipId = apprenticeshipId, Apprenticeship = apprenticeship });

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}
