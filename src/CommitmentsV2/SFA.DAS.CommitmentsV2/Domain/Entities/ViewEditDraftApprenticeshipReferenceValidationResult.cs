using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class ViewEditDraftApprenticeshipReferenceValidationResult
    {
        public ViewEditDraftApprenticeshipReferenceValidationResult()

        {
            Errors = new List<DomainError>();
        }
        public List<DomainError> Errors { get; set; }
    }
}
