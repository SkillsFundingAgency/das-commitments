using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;

public class GetChangeHistoryQueryResult
{
    public List<ChangeHistory> ChangeHistory { get; set; }
}