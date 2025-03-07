using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;

public class GetSupportApprovedApprenticeshipsQueryResult
{
    public IEnumerable<SupportApprenticeshipDetails> ApprovedApprenticeships { get; set; }
}