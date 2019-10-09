using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Shared.Models;

namespace SFA.DAS.CommitmentsV2.Shared.Interfaces
{
    public interface ICommitmentsService
    {
        Task AddDraftApprenticeshipToCohort(long cohortId, AddDraftApprenticeshipRequest request);
        Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request);
        Task<CohortDetails> GetCohortDetail(long cohortId);
        Task<EditDraftApprenticeshipDetails> GetDraftApprenticeshipForCohort(long cohortId, long draftApprenticeshipId);
        Task UpdateDraftApprenticeship(long cohortId, long draftApprenticeshipId, UpdateDraftApprenticeshipRequest updateRequest);
    }
}