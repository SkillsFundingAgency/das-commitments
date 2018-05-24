using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender
{
    public sealed class GetTransferRequestsForSenderQueryHandler : IAsyncRequestHandler<GetTransferRequestsForSenderRequest, GetTransferRequestsForSenderResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        public GetTransferRequestsForSenderQueryHandler(ICommitmentRepository commitmentRepository)
        {
            _commitmentRepository = commitmentRepository;
        }

        public async Task<GetTransferRequestsForSenderResponse> Handle(GetTransferRequestsForSenderRequest message)
        {
            var results = await _commitmentRepository.GetTransferRequestsForSender(message.TransferSenderAccountId);

            return new GetTransferRequestsForSenderResponse
            {
                Data = results
            };
        }

    }
}
