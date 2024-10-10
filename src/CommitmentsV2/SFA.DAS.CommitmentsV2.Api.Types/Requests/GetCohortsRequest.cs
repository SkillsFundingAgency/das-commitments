using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class GetCohortsRequest
{
    public long? AccountId { get; set; }
    public long? ProviderId { get; set; }
}