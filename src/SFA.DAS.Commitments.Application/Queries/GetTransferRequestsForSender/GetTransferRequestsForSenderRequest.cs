using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender
{
    public sealed class GetTransferRequestsForSenderRequest : IAsyncRequest<GetTransferRequestsForSenderResponse>
    {
        public long TransferSenderAccountId { get; set; }
    }
}
