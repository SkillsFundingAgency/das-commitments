using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver
{
    public sealed class GetTransferRequestsForReceiverQueryHandler : IAsyncRequestHandler<GetTransferRequestsForReceiverRequest, GetTransferRequestsForReceiverResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        public GetTransferRequestsForReceiverQueryHandler(ICommitmentRepository commitmentRepository)
        {
            _commitmentRepository = commitmentRepository;
        }

        public async Task<GetTransferRequestsForReceiverResponse> Handle(GetTransferRequestsForReceiverRequest message)
        {
            var results = await _commitmentRepository.GetTransferRequestsForReceiver(message.TransferReceiverAccountId);

            return new GetTransferRequestsForReceiverResponse
            {
                Data = results
            };
        }

    }
}
