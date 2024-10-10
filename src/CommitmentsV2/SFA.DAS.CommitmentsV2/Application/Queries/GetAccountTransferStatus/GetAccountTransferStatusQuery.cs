namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;

public class GetAccountTransferStatusQuery : IRequest<GetAccountTransferStatusQueryResult>
{
    public long AccountId { get; set; }
}