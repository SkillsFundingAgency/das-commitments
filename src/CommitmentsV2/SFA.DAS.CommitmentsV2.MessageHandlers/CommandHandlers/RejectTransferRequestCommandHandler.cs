using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class RejectTransferRequestCommandHandler(ITransferRequestDomainService transferRequestDomainService) : IHandleMessages<RejectTransferRequestCommand>
{
    public async Task Handle(RejectTransferRequestCommand message, IMessageHandlerContext context)
    {
        await transferRequestDomainService.RejectTransferRequest(message.TransferRequestId, message.UserInfo, message.RejectedOn, default);
    }
}