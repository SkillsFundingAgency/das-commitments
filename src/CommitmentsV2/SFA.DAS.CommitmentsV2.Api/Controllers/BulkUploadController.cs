using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Application.Commands.FileUploadLogUpdateWithErrorContent;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/{providerId:long}/bulkupload")]
public class BulkUploadController(IMediator mediator, IModelMapper modelMapper, ILogger<BulkUploadController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddDraftApprenticeships(BulkUploadAddDraftApprenticeshipsRequest request, CancellationToken cancellationToken = default)
    {
        foreach (var df in request.BulkUploadDraftApprenticeships)
        {
            logger.LogInformation("Received Bulk upload request for ULN : {Uln} with start date : {StartDate}.", df.Uln, df.StartDate.Value.ToString("dd/MM/yyyy"));
        }
        
        logger.LogInformation("Received Bulk upload request for Provider : {ProviderId} with number of apprentices : {Count}", request.ProviderId, request.BulkUploadDraftApprenticeships?.Count() ?? 0);
        
        var command = await modelMapper.Map<BulkUploadAddDraftApprenticeshipsCommand>(request);
        var result = await mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    [HttpPost]
    [Route("addandapprove")]
    public async Task<IActionResult> AddAndApproveDraftApprenticeships(BulkUploadAddAndApproveDraftApprenticeshipsRequest request, CancellationToken cancellationToken = default)
    {
        foreach (var df in request.BulkUploadAddAndApproveDraftApprenticeships)
        {
            logger.LogInformation("Received Bulk upload request for ULN : {Uln} with start date : {StartDate}.", df.Uln, df.StartDate.Value.ToString("dd/MM/yyyy"));
        }
        
        logger.LogInformation("Received Bulk upload request for Provider : {ProviderId} with number of apprentices : {Count}", request.ProviderId, request.BulkUploadAddAndApproveDraftApprenticeships?.Count() ?? 0);
        
        var command = await modelMapper.Map<BulkUploadAddAndApproveDraftApprenticeshipsCommand>(request);
        var result = await mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    [HttpPost]
    [Route("validate")]
    public async Task<IActionResult> Validate(BulkUploadValidateApiRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received Bulk upload request for Provider : {ProviderId} with number of apprentices : {CsvRecords}", request.ProviderId, request.CsvRecords?.Count() ?? 0);
        
        var command = await modelMapper.Map<BulkUploadValidateCommand>(request);
        var result = await mediator.Send(command, cancellationToken);
        
        result.BulkUploadValidationErrors.ThrowIfAny();
        
        return Ok(result);
    }

    [HttpPost]
    [Route("/api/providers/{providerId}/learners/{id}/validate")]
    public async Task<IActionResult> Validate(long providerId, long id, ValidateLearnerRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received Validate request for Provider : {providerId} and Learner : {id}", providerId, id);

        var command = new ValidateLearnerCommand {
            ProviderId = providerId,
            LearnerDataId = id,
            LearnerData = request.Learner,
            ProviderStandardsData = request.ProviderStandardsData,
            OtjTrainingHours = null 
        };
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }


    [HttpPost]
    [Route("logs")]
    public async Task<IActionResult> AddLog([FromBody] AddFileUploadLogRequest request, CancellationToken cancellationToken = default)
    {
        var command = await modelMapper.Map<AddFileUploadLogCommand>(request);
        var result = await mediator.Send(command, cancellationToken);
       
        return Ok(result);
    }

    [HttpPut]
    [Route("logs/{logId}/error")]
    public async Task<IActionResult> UpdateLogErrorContent(long providerId, long logId, [FromBody] FileUploadLogUpdateWithErrorContentRequest request, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new FileUploadLogUpdateWithErrorContentCommand
        {
            LogId = logId,
            ProviderId = providerId,
            ErrorContent = request.ErrorContent,
            UserInfo = request.UserInfo
        }, cancellationToken);
        
        return Ok();
    }
}