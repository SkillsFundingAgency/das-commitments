using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands;

public class RejectTransferRequestCommand(long transferRequestId, DateTime rejectedOn, UserInfo userInfo)
{
    public long TransferRequestId { get; } = transferRequestId;
    public DateTime RejectedOn { get; } = rejectedOn;
    public UserInfo UserInfo { get; } = userInfo;
}