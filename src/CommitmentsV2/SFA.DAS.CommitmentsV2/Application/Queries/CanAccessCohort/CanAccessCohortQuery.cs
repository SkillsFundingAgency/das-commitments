using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;

public class CanAccessCohortQuery : IRequest<bool>
{
    public long CohortId { get; set; }
    public Party Party { get; set; }
    public long PartyId { get; set; }
}