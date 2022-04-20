using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{apprenticeshipId}")]
    public class ChangeOfPartyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public ChangeOfPartyController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        [Route("change-of-party-requests")]
        public async Task<IActionResult> GetAll(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetChangeOfPartyRequestsQuery(apprenticeshipId), cancellationToken);
            var response = await _modelMapper.Map<GetChangeOfPartyRequestsResponse>(result);
            return Ok(response);
        }

        [HttpPost]
        [Route("change-of-party-requests")]
        public async Task<IActionResult> CreateChangeOfPartyRequest(long apprenticeshipId, CreateChangeOfPartyRequestRequest request, CancellationToken cancellationToken = default)
        {
            await _mediator.Send(new CreateChangeOfPartyRequestCommand
            {
                ApprenticeshipId = apprenticeshipId,
                ChangeOfPartyRequestType = request.ChangeOfPartyRequestType,
                NewPartyId = request.NewPartyId,
                NewStartDate = request.NewStartDate,
                NewEndDate = request.NewEndDate,
                NewPrice = request.NewPrice,
                UserInfo = request.UserInfo,
                NewEmploymentEndDate = request.NewEmploymentEndDate,
                NewEmploymentPrice = request.NewEmploymentPrice,
            }, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [Route("change-of-provider-chain")]
        public async Task<IActionResult> GetChangeOfProviderChain(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetChangeOfProviderChainQuery(apprenticeshipId), cancellationToken);
            var response = await _modelMapper.Map<GetChangeOfProviderChainResponse>(result);
            return Ok(response);
        }

        [HttpGet]
        [Route("change-of-employer-chain")]
        public async Task<IActionResult> GetChangeOfEmployerChain(long apprenticeshipId, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetChangeOfEmployerChainQuery(apprenticeshipId), cancellationToken);
            var response = await _modelMapper.Map<GetChangeOfEmployerChainResponse>(result);
            return Ok(response);
        }
    }
}
