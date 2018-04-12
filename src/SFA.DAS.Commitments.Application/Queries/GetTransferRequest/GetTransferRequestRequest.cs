using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequest
{
    public sealed class GetTransferRequestRequest : IAsyncRequest<GetTransferRequestResponse>
    {
        public Caller Caller { get; set; }
        public long TransferRequestId { get; set; }
    }
}
