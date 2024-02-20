using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IEditApprenticeshipValidationService
    {
        Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request, CancellationToken cancellationToken);
    }
}
