using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class BulkUploadAddAndApproveDraftApprenticeshipsResponse
{
    public IEnumerable<BulkUploadAddDraftApprenticeshipsResponse> BulkUploadAddAndApproveDraftApprenticeshipResponse { get; set; }
}