using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class RejectTransferRequestCommandHandler : IHandleMessages<RejectTransferRequestCommand>
    {
        private readonly ITransferRequestDomainService _transferRequestDomainService;

        public RejectTransferRequestCommandHandler(ITransferRequestDomainService transferRequestDomainService)
        {
            _transferRequestDomainService = transferRequestDomainService;
        }

        public async Task Handle(RejectTransferRequestCommand message, IMessageHandlerContext context)
        {
            await _transferRequestDomainService.RejectTransferRequest(message.TransferRequestId, message.UserInfo, message.RejectedOn, default);
        }
    }
}
