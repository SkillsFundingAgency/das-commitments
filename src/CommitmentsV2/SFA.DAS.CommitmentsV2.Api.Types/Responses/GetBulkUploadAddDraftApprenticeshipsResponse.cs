using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetBulkUploadAddDraftApprenticeshipsResponse
    {
        public IEnumerable<BulkUploadAddDraftApprenticeshipsResponse> BulkUploadAddDraftApprenticeshipsResponse { get; set; }
    }

    public class BulkUploadAddDraftApprenticeshipsResponse
    {
        public string CohortReference { get; set; }
        public int NumberOfApprenticeships { get; set; }
        public string EmployerName { get; set; }
    }
}
