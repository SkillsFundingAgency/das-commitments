namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;

public class GetTransferRequestQuery : IRequest<GetTransferRequestQueryResult>
{
    public long EmployerAccountId { get; }
    public long TransferRequestId { get; }
    public QueryOriginator Originator { get; }

    public GetTransferRequestQuery(long employerAccountId, long transferRequestId, QueryOriginator originator)
    {
        EmployerAccountId = employerAccountId;
        TransferRequestId = transferRequestId;
        Originator = originator;
    }

    public enum QueryOriginator
    {
        TransferSender,
        TransferReceiver
    }
}