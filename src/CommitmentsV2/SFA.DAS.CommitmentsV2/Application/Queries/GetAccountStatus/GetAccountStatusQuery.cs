namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;

public class GetAccountStatusQuery : IRequest<GetAccountStatusQueryResult>
{
    public long AccountId { get; set; }
    public int CompletionLag { get; set; }
    public int StartLag { get; set; }
    public int NewStartWindow { get; set; }
}