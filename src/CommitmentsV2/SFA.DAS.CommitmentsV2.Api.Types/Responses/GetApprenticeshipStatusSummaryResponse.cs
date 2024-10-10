using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetApprenticeshipStatusSummaryResponse
{
    public IEnumerable<ApprenticeshipStatusSummaryResponse> ApprenticeshipStatusSummaryResponse { get; set; }
}

public class ApprenticeshipStatusSummaryResponse
{
    public string LegalEntityIdentifier { get; set; }
    public OrganisationType LegalEntityOrganisationType { get; set; }

    public int PendingApprovalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PausedCount { get; set; }
    public int WithdrawnCount { get; set; }
    public int CompletedCount { get; set; }
}