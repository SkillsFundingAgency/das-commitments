using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{apprenticeshipId}/change-of-party-requests")]
    public class ChangeOfPartyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public ChangeOfPartyController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpPost]
        [Route("{apprenticeshipId}/change-of-party-requests")]
        public async Task<IActionResult> CreateChangeOfPartyRequest(long apprenticeshipId, CreateChangeOfPartyRequestRequest request, CancellationToken cancellationToken = default)
        {
            await _mediator.Send(new ChangeOfPartyRequestCommand
            {
                ApprenticeshipId = apprenticeshipId,
                ChangeOfPartyRequestType = request.ChangeOfPartyRequestType,
                NewPartyId = request.NewPartyId,
                NewStartDate = request.NewStartDate,
                NewPrice = request.NewPrice,
                UserInfo = request.UserInfo
            }, cancellationToken);

            return Ok();
        }
    }
}
