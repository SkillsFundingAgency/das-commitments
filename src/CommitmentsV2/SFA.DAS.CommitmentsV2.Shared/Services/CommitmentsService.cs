using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class CommitmentsService : ICommitmentsService
    {
        private readonly ICommitmentsApiClient _client;
        private readonly IEncodingService _encodingService;

        public CommitmentsService(ICommitmentsApiClient client, IEncodingService encodingService)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _encodingService = encodingService ?? throw new ArgumentNullException(nameof(encodingService));
        }

        public async Task<CohortDetails> GetCohortDetail(long cohortId)
        {
            var result = await _client.GetCohort(cohortId, CancellationToken.None);

            return new CohortDetails
            {
                CohortId = result.CohortId,
                HashedCohortId = _encodingService.Encode(result.CohortId, EncodingType.CohortReference),
                LegalEntityName = result.LegalEntityName,
                IsFundedByTransfer = result.IsFundedByTransfer,
                ProviderName = result.ProviderName,
                WithParty = result.WithParty
            };
        }

        public Task AddDraftApprenticeshipToCohort(long cohortId, AddDraftApprenticeshipRequest request)
        {
            return _client.AddDraftApprenticeship(cohortId, request);
        }

        public Task<CreateCohortResponse> CreateCohort(CreateCohortRequest request)
        {
            return _client.CreateCohort(request, CancellationToken.None);
        }

        public async Task<EditDraftApprenticeshipDetails> GetDraftApprenticeshipForCohort(long cohortId, long draftApprenticeshipId)
        {
            var result = await _client.GetDraftApprenticeship(cohortId, draftApprenticeshipId);

            return new EditDraftApprenticeshipDetails
            {
                DraftApprenticeshipId = result.Id,
                DraftApprenticeshipHashedId = _encodingService.Encode(result.Id, EncodingType.ApprenticeshipId),
                CohortId = cohortId,
                CohortReference = _encodingService.Encode(cohortId, EncodingType.CohortReference),
                ReservationId = result.ReservationId,
                FirstName = result.FirstName,
                LastName = result.LastName,
                DateOfBirth = result.DateOfBirth,
                UniqueLearnerNumber = result.Uln,
                CourseCode = result.CourseCode,
                Cost = result.Cost,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                OriginatorReference = result.Reference
            };
        }

        public Task UpdateDraftApprenticeship(long cohortId, long draftApprenticeshipId, UpdateDraftApprenticeshipRequest updateRequest)
        {
            return _client.UpdateDraftApprenticeship(cohortId, draftApprenticeshipId, updateRequest);
        }
    }
}