using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class ViewEditDraftApprenticeshipEmailValidationResult
{
    public ViewEditDraftApprenticeshipEmailValidationResult()
    {
        Errors = new List<DomainError>();
    }
    public List<DomainError> Errors { get; set; }
}

