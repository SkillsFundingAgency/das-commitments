namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;

public class GetChangeHistoryQuery : IRequest<GetChangeHistoryQueryResult>
{
    public long ApprenticeshipId { get; set; }
}