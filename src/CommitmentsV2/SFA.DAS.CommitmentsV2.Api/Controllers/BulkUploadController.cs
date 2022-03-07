using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Features;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [DasAuthorize(Feature.BulkUploadV2)]
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
            foreach (var df in request.BulkUploadDraftApprenticeships)
            {
                _logger.LogInformation($"Received Bulk upload request for ULN : {df.Uln} with start date : {df.StartDate.Value.ToString("dd/MM/yyyy")}");
            }
            _logger.LogInformation($"Received Bulk upload request for Provider : {request.ProviderId} with number of apprentices : {request.BulkUploadDraftApprenticeships?.Count() ?? 0}");
            var command = await _modelMapper.Map<BulkUploadAddDraftApprenticeshipsCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("addandapprove")]
        public async Task<IActionResult> AddAndApproveDraftApprenticeships(BulkUploadAddAndApproveDraftApprenticeshipsRequest request, CancellationToken cancellationToken = default)
        {
            foreach (var df in request.BulkUploadAddAndApproveDraftApprenticeships)
            {
                _logger.LogInformation($"Received Bulk upload request for ULN : {df.Uln} with start date : {df.StartDate.Value.ToString("dd/MM/yyyy")}");
            }
            _logger.LogInformation($"Received Bulk upload request for Provider : {request.ProviderId} with number of apprentices : {request.BulkUploadAddAndApproveDraftApprenticeships?.Count() ?? 0}");
            var command = await _modelMapper.Map<BulkUploadAddAndApproveDraftApprenticeshipsCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [Route("validate")]
        public async Task<IActionResult> Validate(BulkUploadValidateApiRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Received Bulk upload request for Provider : {request.ProviderId} with number of apprentices : {request.CsvRecords?.Count() ?? 0}");
            var command = await _modelMapper.Map<BulkUploadValidateCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);
            result.BulkUploadValidationErrors.ThrowIfAny();
            return Ok(result);
        }
    }
}
