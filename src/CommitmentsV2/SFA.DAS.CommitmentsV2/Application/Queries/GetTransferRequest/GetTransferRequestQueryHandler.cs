using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;

public class GetTransferRequestQueryHandler : IRequestHandler<GetTransferRequestQuery, GetTransferRequestQueryResult>
{
    private readonly ITransferRequestDomainService _transferRequestDomainService;

    public GetTransferRequestQueryHandler(ITransferRequestDomainService transferRequestDomainService)
    {
        _transferRequestDomainService = transferRequestDomainService ?? throw new ArgumentNullException(nameof(transferRequestDomainService));
    }

    public async Task<GetTransferRequestQueryResult> Handle(GetTransferRequestQuery message, CancellationToken cancellationToken)
    {
        var result = await _transferRequestDomainService.GetTransferRequest(message.TransferRequestId, message.EmployerAccountId, cancellationToken);
        CheckAuthorization(message, result);
        return result;
    }

    private static void CheckAuthorization(GetTransferRequestQuery message, GetTransferRequestQueryResult transferRequest)
    {
        switch (message.Originator)
        {
            case GetTransferRequestQuery.QueryOriginator.TransferSender:
                if (transferRequest.SendingEmployerAccountId != message.EmployerAccountId)
                    throw new GetTransferRequestQueryException($"Transfer Sender {message.EmployerAccountId} is not authorised to access transfer request {message.TransferRequestId}, expected Sender {transferRequest.SendingEmployerAccountId}");
                break;
            case GetTransferRequestQuery.QueryOriginator.TransferReceiver:
                if (transferRequest.ReceivingEmployerAccountId != message.EmployerAccountId)
                    throw new GetTransferRequestQueryException($"Transfer Receiver {message.EmployerAccountId} is not authorised to access transfer request {message.TransferRequestId}, expected Receiver {transferRequest.ReceivingEmployerAccountId}");
                break;
            default:
                throw new GetTransferRequestQueryException($"Only transfer senders and receivers are allowed to access transfer requests");
        }
    }
}

public class GetTransferRequestQueryException(string message): Exception(message){}