namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;

public class GetSupportApprovedApprenticeshipsQuery(long? cohortId = null, string? uln = null, long? apprenticeshipId = null)
    : IRequest<GetSupportApprovedApprenticeshipsQueryResult>
{
    public long? CohortId { get; } = cohortId;
    public string? Uln { get; } = uln;
    public long? ApprenticeshipId { get; } = apprenticeshipId;
}