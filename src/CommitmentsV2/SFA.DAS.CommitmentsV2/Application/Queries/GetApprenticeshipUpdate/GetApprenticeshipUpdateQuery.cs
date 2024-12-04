using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;

public class GetApprenticeshipUpdateQuery : IRequest<GetApprenticeshipUpdateQueryResult>
{
    public long ApprenticeshipId { get; }

    public ApprenticeshipUpdateStatus? Status { get; }

    public GetApprenticeshipUpdateQuery(long apprenticeshipId, ApprenticeshipUpdateStatus? status)
    {
        ApprenticeshipId = apprenticeshipId;
        Status = status;
    }
}