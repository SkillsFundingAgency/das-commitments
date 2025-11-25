using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class DraftApprenticeshipSetReferenceRequest
{
    public long CohortId { get; set; }
    public long ApprenticeshipId { get; set; }
    public string Reference { get; set; }
    public Party Party { get; set; }
}
