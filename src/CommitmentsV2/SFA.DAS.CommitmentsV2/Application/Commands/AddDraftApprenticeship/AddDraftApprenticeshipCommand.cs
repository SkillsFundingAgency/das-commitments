using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship
{
    public class AddDraftApprenticeshipCommand : DraftApprenticeshipCommandBase, IRequest<AddDraftApprenticeshipResult>
    {
    }
}