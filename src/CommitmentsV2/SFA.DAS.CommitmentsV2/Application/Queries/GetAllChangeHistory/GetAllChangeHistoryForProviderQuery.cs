namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllChangeHistory;

public class GetAllChangeHistoryForProviderQuery : IRequest<GetAllChangeHistoryForProviderQueryResult>
{
    public long ProviderId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}