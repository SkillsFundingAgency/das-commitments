using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcknowledgeInvalidIlrChanges;

public class AcknowledgeInvalidIlrChangesCommand : IRequest
{
    public long ApprenticeshipId { get; set; }
    public long ProviderId { get; set; }
    public UserInfo UserInfo { get; set; }
    public CocApprovalItemStatus ItemStatus { get; set; } = CocApprovalItemStatus.AutoRejected;
    public List<InvalidIlrChangeAcknowledgement> Acknowledgements { get; set; } = [];
}
