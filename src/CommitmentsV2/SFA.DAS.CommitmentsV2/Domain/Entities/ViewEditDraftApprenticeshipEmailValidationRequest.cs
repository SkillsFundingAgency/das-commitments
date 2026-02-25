namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class ViewEditDraftApprenticeshipEmailValidationRequest
{
    public long DraftApprenticeshipId { get; set; }
    public string Email { get; set; }
    public long CohortId { get; set; }
}
