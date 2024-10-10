using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class ApproveTransferRequestCommandHandler(ITransferRequestDomainService transferRequestDomainService) : IHandleMessages<ApproveTransferRequestCommand>
{
    public async Task Handle(ApproveTransferRequestCommand message, IMessageHandlerContext context)
    {
        await transferRequestDomainService.ApproveTransferRequest(message.TransferRequestId, message.UserInfo, message.ApprovedOn, default);
    }
}