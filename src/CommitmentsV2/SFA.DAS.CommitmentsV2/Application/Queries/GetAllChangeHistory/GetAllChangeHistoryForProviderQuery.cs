namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllChangeHistory;

public class GetAllChangeHistoryForProviderQuery : IRequest<GetAllChangeHistoryForProviderQueryResult>
{
    public long ProviderId { get; set; }
}