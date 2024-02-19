using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Application.Commands.FileUploadLogUpdateWithErrorContent;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

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
        foreach (var df in request.BulkUploadDraftApprenticeships)
        {
            _logger.LogInformation("Received Bulk upload request for ULN : {Uln} with start date : {StartDate}.", df.Uln, df.StartDate.Value.ToString("dd/MM/yyyy"));
        }
        _logger.LogInformation("Received Bulk upload request for Provider : {ProviderId} with number of apprentices : {Count}", request.ProviderId, request.BulkUploadDraftApprenticeships?.Count() ?? 0);
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
            _logger.LogInformation("Received Bulk upload request for ULN : {Uln} with start date : {StartDate}.", df.Uln, df.StartDate.Value.ToString("dd/MM/yyyy"));
        }
        _logger.LogInformation("Received Bulk upload request for Provider : {ProviderId} with number of apprentices : {Count}", request.ProviderId, request.BulkUploadAddAndApproveDraftApprenticeships?.Count() ?? 0);
        var command = await _modelMapper.Map<BulkUploadAddAndApproveDraftApprenticeshipsCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Route("validate")]
    public async Task<IActionResult> Validate(BulkUploadValidateApiRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Received Bulk upload request for Provider : {request.ProviderId} with number of apprentices : {request.CsvRecords?.Count() ?? 0}");
        _logger.LogInformation("Received Bulk upload request for Provider : {ProviderId} with number of apprentices : {CsvRecords}", request.ProviderId, request.CsvRecords?.Count() ?? 0);
        var command = await _modelMapper.Map<BulkUploadValidateCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);
        result.BulkUploadValidationErrors.ThrowIfAny();
        return Ok(result);
    }

    [HttpPost]
    [Route("logs")]
    public async Task<IActionResult> AddLog([FromBody] AddFileUploadLogRequest request, CancellationToken cancellationToken = default)
    {
        var command = await _modelMapper.Map<AddFileUploadLogCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Route("logs/{logId}/error")]
    public async Task<IActionResult> UpdateLogErrorContent(long providerId, long logId, [FromBody] FileUploadLogUpdateWithErrorContentRequest request, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new FileUploadLogUpdateWithErrorContentCommand
        {
            LogId = logId,
            ProviderId = providerId,
            ErrorContent = request.ErrorContent,
            UserInfo = request.UserInfo
        }, cancellationToken);
        return Ok();
    }
}