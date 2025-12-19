using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IViewEditDraftApprenticeshipEmailValidationService
{
   Task<ViewEditDraftApprenticeshipEmailValidationResult> Validate(ViewEditDraftApprenticeshipEmailValidationRequest request, CancellationToken cancellationToken);
}
