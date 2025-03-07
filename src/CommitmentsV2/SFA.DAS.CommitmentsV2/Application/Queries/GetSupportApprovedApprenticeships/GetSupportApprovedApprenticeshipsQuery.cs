namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;

public class GetSupportApprovedApprenticeshipsQuery : IRequest<GetSupportApprovedApprenticeshipsQueryResult>
{
    public GetSupportApprovedApprenticeshipsQuery(long? cohortId, string uln, long? apprenticeshipId)
    {
        CohortId = cohortId;
        Uln = uln;
        ApprenticeshipId = apprenticeshipId;
    }
    public long? CohortId { get; }
    public string Uln { get; }
    public long? ApprenticeshipId { get; }
}