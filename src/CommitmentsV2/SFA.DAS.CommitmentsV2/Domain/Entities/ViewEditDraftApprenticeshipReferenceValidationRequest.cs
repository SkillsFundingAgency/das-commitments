using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class ViewEditDraftApprenticeshipReferenceValidationRequest
{
    public long DraftApprenticeshipId { get; set; }
    public string Reference { get; set; }
    public long CohortId { get; set; }
    public Party Party { get; set; }
}
