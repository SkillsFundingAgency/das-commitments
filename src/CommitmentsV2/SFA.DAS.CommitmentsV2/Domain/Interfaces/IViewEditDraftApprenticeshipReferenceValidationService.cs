using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IViewEditDraftApprenticeshipReferenceValidationService
{
    Task<ViewEditDraftApprenticeshipReferenceValidationResult> Validate(ViewEditDraftApprenticeshipReferenceValidationRequest request, CancellationToken cancellationToken);
}
