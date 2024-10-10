using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class AccountResponse
{
    public long AccountId { get; set; }
    public bool HasCohorts { get; set; }
    public bool HasApprenticeships { get; set; }
    public ApprenticeshipEmployerType LevyStatus { get; set; }
}