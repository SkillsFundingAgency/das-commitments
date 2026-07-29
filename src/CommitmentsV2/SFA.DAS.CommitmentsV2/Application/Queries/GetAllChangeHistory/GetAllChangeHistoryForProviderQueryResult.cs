using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllChangeHistory;

public class GetAllChangeHistoryForProviderQueryResult
{
    public List<ChangeHistory> ChangeHistory { get; set; } = [];
}