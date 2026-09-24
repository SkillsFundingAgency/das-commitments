using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetInvalidIlrChanges;

public class GetInvalidIlrChangesQuery : IRequest<GetInvalidIlrChangesResponse>
{
    public const string AutoRejectedDecision = "Auto rejected";
    public const string EmployerRejectedDecision = "Declined";

    public long ApprenticeshipId { get; set; }
    public long ProviderId { get; set; }
    public CocApprovalItemStatus ItemStatus { get; set; } = CocApprovalItemStatus.AutoRejected;
    public string Decision { get; set; } = AutoRejectedDecision;
}
