using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;

public class AddTransferRequestCommand : IRequest
{
    public long CohortId { get; set; }
    public Party LastApprovedByParty { get; set; }
}