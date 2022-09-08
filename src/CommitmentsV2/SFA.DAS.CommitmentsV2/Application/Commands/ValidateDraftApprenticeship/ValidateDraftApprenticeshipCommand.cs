using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeship
{
    public class ValidateDraftApprenticeshipCommand : DraftApprenticeshipCommandBase, IRequest<ValidateDraftApprenticeshipResult>
    {
    }
}