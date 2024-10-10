using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class ApprenticeshipAccessRequest
{
    public Party Party { get; set; }
    public long PartyId { get; set; }
    public long ApprenticeshipId { get; set; }
}