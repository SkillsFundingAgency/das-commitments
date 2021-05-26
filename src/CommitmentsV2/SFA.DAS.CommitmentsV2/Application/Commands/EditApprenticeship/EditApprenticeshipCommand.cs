using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship
{
    public class EditApprenticeshipCommand : IRequest<EditApprenticeshipResponse>
    {
        public EditApprenticeshipApiRequest EditApprenticeshipRequest { get; set; }
    }
}
