using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class ValidateSelectMultipleLearnersApiResponse
{
    public IEnumerable<BulkUploadValidationError> ValidationErrors { get; set; } = new List<BulkUploadValidationError>();
}