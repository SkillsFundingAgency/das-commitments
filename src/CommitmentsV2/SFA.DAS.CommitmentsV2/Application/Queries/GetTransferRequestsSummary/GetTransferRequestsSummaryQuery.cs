using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;

public class GetTransferRequestsSummaryQuery : IRequest<GetTransferRequestsSummaryQueryResult>
{
    public long AccountId { get; }
    public TransferType? Originator { get; }

    public GetTransferRequestsSummaryQuery(long accountId, TransferType? originator)
    {
        AccountId = accountId;
        Originator = originator;
    }
}