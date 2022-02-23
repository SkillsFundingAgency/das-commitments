using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class BulkUploadAddAndApproveDraftApprenticeshipsResponse
    {
        public IEnumerable<BulkUploadAddAndApproveDraftApprenticeshipResponse> BulkUploadAddAndApproveDraftApprenticeshipResponse { get; set; }
    }

    public class BulkUploadAddAndApproveDraftApprenticeshipResponse
    {
        public string CohortReference { get; set; } 
        public int NumberOfApprenticeships { get; set; }
        public string EmployerName { get; set; }
    }
}
