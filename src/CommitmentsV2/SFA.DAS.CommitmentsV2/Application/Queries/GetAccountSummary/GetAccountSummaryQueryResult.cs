using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;

public class GetAccountSummaryQueryResult
{
    public long AccountId { get; set; }
    public ApprenticeshipEmployerType LevyStatus { get; set; }
}