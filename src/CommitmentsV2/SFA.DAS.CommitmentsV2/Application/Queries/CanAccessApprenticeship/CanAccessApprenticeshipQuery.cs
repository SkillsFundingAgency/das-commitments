using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;

public class CanAccessApprenticeshipQuery : IRequest<bool>
{
    public long ApprenticeshipId { get; set; }
    public Party Party { get; set; }
    public long PartyId { get; set; }
}