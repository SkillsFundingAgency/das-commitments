using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetInvalidIlrChanges;

public class GetInvalidIlrChangesQuery : IRequest<GetInvalidIlrChangesResponse>
{
    public long ApprenticeshipId { get; set; }
    public long ProviderId { get; set; }
}
