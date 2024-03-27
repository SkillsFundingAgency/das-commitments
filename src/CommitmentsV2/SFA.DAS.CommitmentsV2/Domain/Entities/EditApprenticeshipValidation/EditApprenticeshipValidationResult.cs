using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation
{
    public class EditApprenticeshipValidationResult
    {
        public EditApprenticeshipValidationResult()
        {
            Errors = new List<DomainError>();
        }

        public List<DomainError> Errors { get; set; }
    }
}
