using System;
using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Commands.VerifyRelationship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ProviderOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public ProviderOrchestrator(IMediator mediator, ICommitmentsLogger logger)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _mediator = mediator;
            _logger = logger;
        }

        public async Task<GetCommitmentsResponse> GetCommitments(long providerId)
        {
            _logger.Info($"Getting commitments for provider {providerId}", providerId: providerId);

            return await _mediator.SendAsync(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });
        }

        public async Task<GetCommitmentResponse> GetCommitment(long providerId, long commitmentId)
        {
            _logger.Info($"Getting commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            return await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId
            });
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long providerId)
        {
            _logger.Info($"Getting apprenticeships for provider {providerId}", providerId: providerId);

            return await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });
        }

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long providerId, long apprenticeshipId)
        {
            _logger.Info($"Getting apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            return await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId
            });
        }

        public async Task<long> CreateApprenticeship(long providerId, long commitmentId, ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Info($"Creating apprenticeship for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            return await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId
            });
        }

        public async Task PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Info($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId
            });
        }

        public async Task CreateApprenticeships(long providerId, long commitmentId, BulkApprenticeshipRequest bulkRequest)
        {
            _logger.Info($"Bulk uploading {bulkRequest.Apprenticeships?.Count ?? 0} apprenticeships for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller(providerId, CallerType.Provider),
                CommitmentId = commitmentId,
                Apprenticeships = bulkRequest.Apprenticeships,
                UserId = bulkRequest.UserId
            });
        }

        public async Task PatchCommitment(long providerId, long commitmentId, CommitmentSubmission submission)
        {
            _logger.Info($"Updating latest action to {submission.Action} for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                LatestAction = submission.Action,
                LastUpdatedByName = submission.LastUpdatedByInfo.Name,
                LastUpdatedByEmail = submission.LastUpdatedByInfo.EmailAddress,
                UserId = submission.UserId
            });
        }

        public async Task DeleteApprenticeship(long providerId, long apprenticeshipId, string userId)
        {
            _logger.Info($"Deleting apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new DeleteApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId,
                UserId = userId
            });
        }

        public async Task DeleteCommitment(long providerId, long commitmentId, string userId)
        {
            _logger.Info($"Deleting commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new DeleteCommitmentCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                UserId = userId
            });
        }

        public async Task<GetRelationshipResponse> GetRelationship(long providerId, long employerAccountId, string legalEntityId)
        {
            _logger.Info($"Getting relationship for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);

            return await _mediator.SendAsync(new GetRelationshipRequest
            {
                ProviderId = providerId,
                EmployerAccountId = employerAccountId,
                LegalEntityId = legalEntityId
            });
        }

        public async Task<GetRelationshipByCommitmentResponse> GetRelationship(long providerId, long commitmentId)
        {
            _logger.Info($"Getting relationship for provider {providerId}, commitment {commitmentId}", null, providerId, commitmentId);

            return await _mediator.SendAsync(new GetRelationshipByCommitmentRequest
            {
                ProviderId = providerId,
                CommitmentId = commitmentId
            });
        }

        public async Task PatchRelationship(long providerId, long employerAccountId, string legalEntityId, RelationshipRequest patchRequest)
        {
            _logger.Info($"Verifying relationship for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);

            await _mediator.SendAsync(new VerifyRelationshipCommand
            {
                ProviderId = providerId,
                EmployerAccountId = employerAccountId,
                LegalEntityId = legalEntityId,
                UserId = patchRequest.UserId,
                Verified = patchRequest.Relationship.Verified
            });
        }

        public async Task<GetPendingApprenticeshipUpdateResponse> GetPendingApprenticeshipUpdate(long providerId, long apprenticeshipId)
        {
            _logger.Info($"Getting pending update for apprenticeship {apprenticeshipId} for provider account {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            var response = await _mediator.SendAsync(new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId
            });

            return response;
        }

        public async Task CreateApprenticeshipUpdate(long providerId, ApprenticeshipUpdateRequest updateRequest)
        {
            _logger.Info($"Creating update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for provider account {providerId}", providerId: providerId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);

            await _mediator.SendAsync(new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipUpdate = updateRequest.ApprenticeshipUpdate
            });
        }
    }
}
