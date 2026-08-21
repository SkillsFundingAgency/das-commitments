using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcknowledgeInvalidIlrChanges;

public class AcknowledgeInvalidIlrChangesCommand : IRequest
{
    public long ApprenticeshipId { get; set; }
    public long ProviderId { get; set; }
    public UserInfo UserInfo { get; set; }
    public List<InvalidIlrChangeAcknowledgement> Acknowledgements { get; set; } = [];
}
