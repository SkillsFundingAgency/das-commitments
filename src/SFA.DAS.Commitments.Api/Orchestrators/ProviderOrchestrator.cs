using System;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Api.Models;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.CreateBulkUpload;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Commands.VerifyRelationship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetBulkUploadFile;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

using Originator = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.Originator;
using PaymentStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.PaymentStatus;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ProviderOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        private readonly FacetMapper _facetMapper;

        private readonly ApprenticeshipFilterService _apprenticeshipFilterService;

        public ProviderOrchestrator(
            IMediator mediator, 
            ICommitmentsLogger logger,
            FacetMapper facetMapper,
            ApprenticeshipFilterService apprenticeshipFilterService)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (facetMapper == null)
                throw new ArgumentNullException(nameof(facetMapper));
            if (apprenticeshipFilterService == null)
                throw new ArgumentNullException(nameof(apprenticeshipFilterService));

            _mediator = mediator;
            _logger = logger;
            _facetMapper = facetMapper;
            _apprenticeshipFilterService = apprenticeshipFilterService;
        }

        public async Task<GetCommitmentsResponse> GetCommitments(long providerId)
        {
            _logger.Trace($"Getting commitments for provider {providerId}", providerId: providerId);

            var response = await _mediator.SendAsync(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            _logger.Info($"Retrieved commitments for provider {providerId}. {response.Data?.Count} commitments found", providerId: providerId, recordCount: response.Data?.Count);

            return response;
        }

        public async Task<GetCommitmentResponse> GetCommitment(long providerId, long commitmentId)
        {
            _logger.Trace($"Getting commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            var response = await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId
            });

            _logger.Info($"Retrieved commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            return response;
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long providerId)
        {
            _logger.Trace($"Getting apprenticeships for provider {providerId}", providerId: providerId);

            var response = await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            _logger.Info($"Retrieved apprenticeships for provider {providerId}. {response.Data.Count} apprenticeships found", providerId: providerId, recordCount: response.Data.Count);

            return response;
        }

        public async Task<ApprenticeshipSearchResponse> GetApprenticeships(long providerId, ApprenticeshipSearchQuery query)
        {
            _logger.Trace($"Getting apprenticeships with filter query for provider {providerId}", providerId: providerId);

            var request = new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId,
                },
                PageNumber = query.PageNumber == 0 ? GetApprenticeshipsRequest.DefaultPageNumber : query.PageNumber,
                PageSize = query.PageSize == 0 ? GetApprenticeshipsRequest.DefaultPageSize : query.PageSize
            };

            var response = await _mediator.SendAsync(request);

            var approvedApprenticeships = response.Data
                .Where(m => m.PaymentStatus != PaymentStatus.PendingApproval).ToList();

            var facets = _facetMapper.BuildFacets(approvedApprenticeships, query, Originator.Provider);

            var filteredProviders = _apprenticeshipFilterService.Filter(approvedApprenticeships, query, Originator.Provider);

            return new ApprenticeshipSearchResponse
            {
                Apprenticeships = filteredProviders,
                Facets = facets,
                TotalApprenticeships = approvedApprenticeships.Count,
                PageNumber = DeterminePageNumberBasedOnTotalHits(request, approvedApprenticeships),
                PageSize = request.PageSize
            };
        }

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long providerId, long apprenticeshipId)
        {
            _logger.Trace($"Getting apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            var response = await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId
            });

            if (response.Data != null)
                _logger.Info($"Retrieved apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId, commitmentId: response.Data.CommitmentId);
            else
                _logger.Info($"Couldn't find apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            return response;
        }

        public async Task<long> CreateApprenticeship(long providerId, long commitmentId, ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Trace($"Creating apprenticeship for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            var id = await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId,
                UserName = apprenticeshipRequest.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Created apprenticeship {id} for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, apprenticeshipId: id, recordCount: 1);

            return id;
        }

        public async Task PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Trace($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

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
                UserId = apprenticeshipRequest.UserId,
                UserName = apprenticeshipRequest.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Updated apprenticeship {apprenticeshipId} in commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

        }

        public async Task CreateApprenticeships(long providerId, long commitmentId, BulkApprenticeshipRequest bulkRequest)
        {
            _logger.Trace($"Bulk uploading {bulkRequest.Apprenticeships?.Count ?? 0} apprenticeships for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller(providerId, CallerType.Provider),
                CommitmentId = commitmentId,
                Apprenticeships = bulkRequest.Apprenticeships,
                UserId = bulkRequest.UserId,
                UserName = bulkRequest.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Bulk uploaded {bulkRequest.Apprenticeships?.Count ?? 0} apprenticeships for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, recordCount: bulkRequest.Apprenticeships?.Count ?? 0);

        }

        public async Task PatchCommitment(long providerId, long commitmentId, CommitmentSubmission submission)
        {
            _logger.Trace($"Updating latest action to {submission.Action} for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

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
                UserId = submission.UserId,
                Message = submission.Message
            });

            _logger.Info($"Updated latest action to {submission.Action} for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);
        }

        public async Task DeleteApprenticeship(long providerId, long apprenticeshipId, string userId, string userName)
        {
            _logger.Trace($"Deleting apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new DeleteApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId,
                UserId = userId,
                UserName = userName
            });

            _logger.Info($"Deleted apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);
        }

        public async Task DeleteCommitment(long providerId, long commitmentId, string userId, string userName)
        {
            _logger.Trace($"Deleting commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new DeleteCommitmentCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                UserId = userId,
                UserName = userName
            });

            _logger.Info($"Deleted commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);
        }

        public async Task<GetRelationshipResponse> GetRelationship(long providerId, long employerAccountId, string legalEntityId)
        {
            _logger.Trace($"Getting relationship for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);

            var response = await _mediator.SendAsync(new GetRelationshipRequest
            {
                ProviderId = providerId,
                EmployerAccountId = employerAccountId,
                LegalEntityId = legalEntityId
            });

            if (response.Data != null)
                _logger.Info($"Retrieved relationship for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);
            else
                _logger.Info($"Relationship not found for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);

            return response;
        }

        public async Task<GetRelationshipByCommitmentResponse> GetRelationship(long providerId, long commitmentId)
        {
            _logger.Trace($"Getting relationship for provider {providerId}, commitment {commitmentId}", null, providerId, commitmentId);

            var response = await _mediator.SendAsync(new GetRelationshipByCommitmentRequest
            {
                ProviderId = providerId,
                CommitmentId = commitmentId
            });

            if (response.Data != null)
                _logger.Info($"Getting relationship for provider {providerId}, commitment {commitmentId}", null, providerId, commitmentId);
            else
                _logger.Info($"Relationship not found for provider {providerId}, commitment {commitmentId}", null, providerId, commitmentId);

            return response;
        }

        public async Task PatchRelationship(long providerId, long employerAccountId, string legalEntityId, RelationshipRequest patchRequest)
        {
            _logger.Trace($"Verifying relationship for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);

            await _mediator.SendAsync(new VerifyRelationshipCommand
            {
                ProviderId = providerId,
                EmployerAccountId = employerAccountId,
                LegalEntityId = legalEntityId,
                UserId = patchRequest.UserId,
                Verified = patchRequest.Relationship.Verified
            });

            _logger.Info($"Verified relationship for provider {providerId}, employer {employerAccountId}, legal entity {legalEntityId}", employerAccountId, providerId);
        }

        public async Task<GetPendingApprenticeshipUpdateResponse> GetPendingApprenticeshipUpdate(long providerId, long apprenticeshipId)
        {
            _logger.Trace($"Getting pending update for apprenticeship {apprenticeshipId} for provider account {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            var response = await _mediator.SendAsync(new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = apprenticeshipId
            });

            _logger.Info($"Retrieved pending update for apprenticeship {apprenticeshipId} for provider {providerId}", providerId, apprenticeshipId: apprenticeshipId);

            return response;
        }

        public async Task CreateApprenticeshipUpdate(long providerId, ApprenticeshipUpdateRequest updateRequest)
        {
            _logger.Trace($"Creating update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for provider account {providerId}", providerId: providerId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);

            await _mediator.SendAsync(new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipUpdate = updateRequest.ApprenticeshipUpdate,
                UserName = updateRequest.LastUpdatedByInfo?.Name,
                UserId = updateRequest.UserId
            });

            _logger.Info($"Created update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for provider {providerId}", providerId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);
        }

        public async Task PatchApprenticeshipUpdate(long providerId, long apprenticeshipId, ApprenticeshipUpdateSubmission submission)
        {
            _logger.Trace($"Patching update for apprenticeship {apprenticeshipId} for provider {providerId} with status {submission.UpdateStatus}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            var command =
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = apprenticeshipId,
                    Caller = new Caller(providerId, CallerType.Provider),
                    UserId = submission.UserId,
                    UpdateStatus = (ApprenticeshipUpdateStatus)submission.UpdateStatus,
                    UserName = submission.LastUpdatedByInfo?.Name
                };

            await _mediator.SendAsync(command);

            _logger.Info($"Patched update for apprenticeship {apprenticeshipId} for provider {providerId} with status {submission.UpdateStatus}", providerId, apprenticeshipId: apprenticeshipId);
        }
                
        public async Task<long> PostBulkUploadFile(long providerId, BulkUploadFileRequest bulkUploadFile)
        {
            _logger.Trace($"Saving bulk upload file for provider {providerId} ", providerId: providerId);

            var result = await _mediator.SendAsync(
                new CreateBulkUploadCommand
                {
                    ProviderId = providerId,
                    CommitmentId = bulkUploadFile.CommitmentId,
                    FileName = bulkUploadFile.FileName,
                    BulkUploadFile = bulkUploadFile.Data
                });

            return result;
        }

        public async Task<string> GettBulkUploadFile(long providerId, long bulkUploadFileId)
        {
            _logger.Trace($"Saving bulk upload file for provider {providerId}, FileId {bulkUploadFileId} ", providerId: providerId);

            var result = await _mediator.SendAsync(new GetBulkUploadFileQuery { ProviderId = providerId, BulkUploadFileId = bulkUploadFileId });

            return result.Data;
        }

        private static int DeterminePageNumberBasedOnTotalHits(GetApprenticeshipsRequest request, List<Types.Apprenticeship.Apprenticeship> approvedApprenticeships)
        {
            if (request.PageNumber == 0 || request.PageNumber > (approvedApprenticeships.Count + request.PageSize - 1) / request.PageSize)
                return 1;

            return request.PageNumber;
        }
    }
}
