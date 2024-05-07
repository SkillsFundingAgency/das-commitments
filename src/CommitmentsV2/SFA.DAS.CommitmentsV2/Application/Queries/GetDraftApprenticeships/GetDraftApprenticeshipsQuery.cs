namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsQuery : IRequest<GetDraftApprenticeshipsQueryResult>
    {
        public GetDraftApprenticeshipsQuery(long cohortId)
        {
            CohortId = cohortId;
        }

        public long CohortId { get; set; }
    }
}
