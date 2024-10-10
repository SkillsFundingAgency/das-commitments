using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class GetApprenticeshipUpdatesRequest
{
    public ApprenticeshipUpdateStatus? Status { get; set; }
}