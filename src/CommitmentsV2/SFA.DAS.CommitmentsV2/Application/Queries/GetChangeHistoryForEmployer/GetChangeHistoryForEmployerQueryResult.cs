using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistoryForEmployer;

public class GetChangeHistoryForEmployerQueryResult
{
    public List<ChangeHistory> ChangeHistory { get; set; } = [];
}