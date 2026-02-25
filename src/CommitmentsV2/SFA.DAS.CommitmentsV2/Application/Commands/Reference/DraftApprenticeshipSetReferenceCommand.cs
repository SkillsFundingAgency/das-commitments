using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Reference;

public class DraftApprenticeshipSetReferenceCommand : IRequest
{
    public long CohortId { get; set; }
    public long ApprenticeshipId { get; set; }
    public string Reference { get; set; }
    public Party Party { get; set; }
}
