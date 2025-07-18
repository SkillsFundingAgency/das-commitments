
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class EditApprenticeshipCommand(EditApprenticeshipApiRequest editApprenticeshipRequest, Party party)
    : IRequest<EditApprenticeshipResponse>
{
    public EditApprenticeshipApiRequest EditApprenticeshipRequest { get; set; } = editApprenticeshipRequest;

    public Party Party { get; } = party;

    public EditApprenticeshipCommand() : this(null, Party.None)
    {
    }
}