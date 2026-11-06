namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;

public class GetApprenticeshipLockedFieldsQuery : IRequest<GetApprenticeshipLockedFieldsQueryResult>
{
    public long ApprenticeshipId { get; }

    public GetApprenticeshipLockedFieldsQuery(long apprenticeshipId)
    {
        ApprenticeshipId = apprenticeshipId;
    }
}