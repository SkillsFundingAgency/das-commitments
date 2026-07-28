namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistoryForEmployer;

public class GetChangeHistoryForEmployerQuery : IRequest<GetChangeHistoryForEmployerQueryResult>
{
    public long AccountId { get; set; }
}