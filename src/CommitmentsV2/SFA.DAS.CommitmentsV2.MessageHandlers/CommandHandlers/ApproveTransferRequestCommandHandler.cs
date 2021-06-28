using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class ApproveTransferRequestCommandHandler : IHandleMessages<ApproveTransferRequestCommand>
    {
        private readonly ITransferRequestDomainService _transferRequestDomainService;

        public ApproveTransferRequestCommandHandler(ITransferRequestDomainService transferRequestDomainService)
        {
            _transferRequestDomainService = transferRequestDomainService;
        }

        public async Task Handle(ApproveTransferRequestCommand message, IMessageHandlerContext context)
        {
            await _transferRequestDomainService.ApproveTransferRequest(message.TransferRequestId, message.UserInfo, message.ApprovedOn, default);
        }
    }
}