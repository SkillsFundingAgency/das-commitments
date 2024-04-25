using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship
{
    public class EditApprenticeshipCommand : IRequest<EditApprenticeshipResponse>
    {
        public EditApprenticeshipApiRequest EditApprenticeshipRequest { get; set; }

        public Party Party { get; }

        public EditApprenticeshipCommand()
        {
            Party = Party.None;
        }
        public EditApprenticeshipCommand(EditApprenticeshipApiRequest editApprenticeshipRequest, Party party)
        {
            EditApprenticeshipRequest = editApprenticeshipRequest;
            Party = party;
        }
    }
}