using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver
{
    public sealed class GetTransferRequestsForReceiverRequest : IAsyncRequest<GetTransferRequestsForReceiverResponse>
    {
        public long TransferReceiverAccountId { get; set; }
    }
}
