using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/{providerId}/bulkupload")]
    public class BulkUploadController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;
        private readonly ILogger<BulkUploadController> _logger;

        public BulkUploadController(IMediator mediator, IModelMapper modelMapper, ILogger<BulkUploadController> logger)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
            _logger = logger;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddDraftApprenticeships(BulkUploadAddDraftApprenticeshipsRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Received Bulk upload request for Provider : {request.ProviderId} with number of apprentices : {request.BulkUploadDraftApprenticeships?.Count() ?? 0}");
            var command = await _modelMapper.Map<BulkUploadAddDraftApprenticeshipsCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

    }
}
