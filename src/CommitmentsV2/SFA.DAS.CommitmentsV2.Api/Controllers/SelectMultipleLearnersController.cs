using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.ValidateSelectMultipleLearnersRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/{providerId:long}/selectmultiplelearners")]
public class SelectMultipleLearnersController(IMediator mediator, IModelMapper modelMapper, ILogger<BulkUploadController> logger)
    : ControllerBase
{

    [HttpPost]
    [Route("validate")]
    public async Task<IActionResult> Validate(ValidateSelectMultipleLearnersRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received ValidateSelectMultipleLearnersRequest for Provider : {ProviderId} with number of apprentices : {CsvRecords}", request.ProviderId, request.CsvRecords?.Count() ?? 0);

        var command = await modelMapper.Map<ValidateSelectMultipleLearnersQuery>(request);
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}