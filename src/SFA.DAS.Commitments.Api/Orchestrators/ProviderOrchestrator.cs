using System;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.CreateBulkUpload;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetBulkUploadFile;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

using Originator = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.Originator;
using PaymentStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.PaymentStatus;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange;
using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort;
using SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements;
using SFA.DAS.Commitments.Application.Queries.GetProvider;
using SFA.DAS.Commitments.Domain.Entities;

using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship.Apprenticeship;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ProviderOrchestrator : IProviderOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;
        private readonly ICommitmentMapper _commitmentMapper;
        private readonly FacetMapper _facetMapper;
        private readonly ApprenticeshipFilterService _apprenticeshipFilterService;

        public ProviderOrchestrator(
            IMediator mediator, 
            ICommitmentsLogger logger,
            FacetMapper facetMapper,
            ApprenticeshipFilterService apprenticeshipFilterService,
            IApprenticeshipMapper apprenticeshipMapper,
            ICommitmentMapper commitmentMapper)
        {
            _mediator = mediator;
            _logger = logger;
            _facetMapper = facetMapper;
            _apprenticeshipFilterService = apprenticeshipFilterService;
            _apprenticeshipMapper = apprenticeshipMapper;
            _commitmentMapper = commitmentMapper;
        }

        public async Task<IEnumerable<CommitmentListItem>> GetCommitments(long providerId)
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

            return _commitmentMapper.MapFrom(response.Data, CallerType.Provider);
        }

        public async Task<IEnumerable<Types.Commitment.CommitmentAgreement>> GetCommitmentAgreements(long providerId)
        {
            _logger.Trace($"Getting agreement commitments for provider {providerId}", providerId: providerId);

            var response = await _mediator.SendAsync(new GetCommitmentAgreementsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            _logger.Info($"Retrieved agreement commitments for provider {providerId}. {response.Data?.Count} commitments found", providerId: providerId, recordCount: response.Data?.Count);

            return response.Data.Select(_commitmentMapper.Map);
        }

        public async Task<CommitmentView> GetCommitment(long providerId, long commitmentId)
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

            return _commitmentMapper.MapFrom(response.Data, CallerType.Provider);
        }

        public async Task<IEnumerable<Apprenticeship>> GetApprenticeships(long providerId)
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

            _logger.Info($"Retrieved apprenticeships for provider {providerId}. {response.Apprenticeships.Count} apprenticeships found", providerId: providerId, recordCount: response.Apprenticeships.Count);

            return _apprenticeshipMapper.MapFrom(response.Apprenticeships, CallerType.Provider);
        }

        public async Task<ApprenticeshipSearchResponse> GetApprenticeships(long providerId, ApprenticeshipSearchQuery query)
        {
            _logger.Trace($"Getting apprenticeships with filter query for provider {providerId}. Page: {query.PageNumber}, PageSize: {query.PageSize}", providerId: providerId);
            _logger.Info($"Searching for {query.SearchKeyword} by Provider {providerId}", providerId: providerId);

            var response = await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                }
            });

            var apiApprenticeships = _apprenticeshipMapper.MapFromV2(response.Apprenticeships, CallerType.Provider).ToList();

            var totalApprenticeshipsBeforeFilter = response.TotalCount - apiApprenticeships.Count(m => m.PaymentStatus == PaymentStatus.PendingApproval);
            var approvedApprenticeships = apiApprenticeships
                .Where(m => m.PaymentStatus != PaymentStatus.PendingApproval).ToList();

            _logger.Info($"Searching for {query.SearchKeyword} by Provider {providerId}", providerId: providerId);

            var facets = _facetMapper.BuildFacets(approvedApprenticeships, query, Originator.Provider);
            var filteredApprenticeships = _apprenticeshipFilterService.Filter(approvedApprenticeships, query, Originator.Provider);

            _logger.Info($"Retrieved {approvedApprenticeships.Count} apprenticeships with filter query for provider {providerId}. Page: {query.PageNumber}, PageSize: {query.PageSize}", providerId: providerId);

            return new ApprenticeshipSearchResponse
            {
                Apprenticeships = filteredApprenticeships.PageOfResults,
                SearchKeyword = query.SearchKeyword,
                Facets = facets,
                TotalApprenticeships = filteredApprenticeships.TotalResults,
                TotalApprenticeshipsBeforeFilter = totalApprenticeshipsBeforeFilter,
                PageNumber = filteredApprenticeships.PageNumber,
                PageSize = filteredApprenticeships.PageSize
            };
        }

        public async Task<Apprenticeship> GetApprenticeship(long providerId, long apprenticeshipId)
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

            if (response.Data == null)
            {
                _logger.Info($"Couldn't find apprenticeship {apprenticeshipId} for provider {providerId}", providerId, apprenticeshipId: apprenticeshipId);
                return null;
            }

            _logger.Info($"Retrieved apprenticeship {apprenticeshipId} for provider {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId, commitmentId: response.Data.CommitmentId);

            return _apprenticeshipMapper.MapFrom(response.Data, CallerType.Provider);
        }

        public async Task CreateApprenticeships(long providerId, long commitmentId, BulkApprenticeshipRequest bulkRequest)
        {
            _logger.Trace($"Bulk uploading {bulkRequest.Apprenticeships?.Count ?? 0} apprenticeships for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller(providerId, CallerType.Provider),
                CommitmentId = commitmentId,
                Apprenticeships = bulkRequest.Apprenticeships.Select(x => _apprenticeshipMapper.Map(x, CallerType.Provider)),
                UserId = bulkRequest.UserId,
                UserName = bulkRequest.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Bulk uploaded {bulkRequest.Apprenticeships?.Count} apprenticeships for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId, recordCount: bulkRequest.Apprenticeships?.Count);

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
                LatestAction = (LastAction)submission.Action,
                LastUpdatedByName = submission.LastUpdatedByInfo.Name,
                LastUpdatedByEmail = submission.LastUpdatedByInfo.EmailAddress,
                UserId = submission.UserId,
                Message = submission.Message
            });

            _logger.Info($"Updated latest action to {submission.Action} for commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);
        }

        public async Task ApproveCohort(long providerId, long commitmentId, CommitmentSubmission submission)
        {
            _logger.Trace($"Approving commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);

            await _mediator.SendAsync(new ProviderApproveCohortCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                CommitmentId = commitmentId,
                LastUpdatedByName = submission.LastUpdatedByInfo.Name,
                LastUpdatedByEmail = submission.LastUpdatedByInfo.EmailAddress,
                UserId = submission.UserId,
                Message = submission.Message
            });

            _logger.Info($"Approved commitment {commitmentId} for provider {providerId}", providerId: providerId, commitmentId: commitmentId);
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

        public async Task<Types.Apprenticeship.ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long providerId, long apprenticeshipId)
        {
            _logger.Trace($"Getting pending update for apprenticeship {apprenticeshipId} for provider account {providerId}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            var response = await _mediator.SendAsync(new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller { CallerType = CallerType.Provider, Id = providerId },
                ApprenticeshipId = apprenticeshipId
            });

            _logger.Info($"Retrieved pending update for apprenticeship {apprenticeshipId} for provider {providerId}", providerId, apprenticeshipId: apprenticeshipId);

            return _apprenticeshipMapper.MapApprenticeshipUpdate(response?.Data);
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
                ApprenticeshipUpdate = _apprenticeshipMapper.MapApprenticeshipUpdate(updateRequest.ApprenticeshipUpdate),
                UserName = updateRequest.LastUpdatedByInfo?.Name,
                UserId = updateRequest.UserId
            });

            _logger.Info($"Created update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for provider {providerId}", providerId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);
        }

        public async Task PatchApprenticeshipUpdate(long providerId, long apprenticeshipId, ApprenticeshipUpdateSubmission submission)
        {
            _logger.Trace($"Patching update for apprenticeship {apprenticeshipId} for provider {providerId} with status {submission.UpdateStatus}", providerId: providerId, apprenticeshipId: apprenticeshipId);

            switch (submission.UpdateStatus)
            {
                case Types.Apprenticeship.Types.ApprenticeshipUpdateStatus.Approved:
                    await _mediator.SendAsync(new AcceptApprenticeshipChangeCommand
                    {
                        ApprenticeshipId = apprenticeshipId,
                        Caller = new Caller(providerId, CallerType.Provider),
                        UserId = submission.UserId,
                        UserName = submission.LastUpdatedByInfo?.Name
                    });
                    break;
                case Types.Apprenticeship.Types.ApprenticeshipUpdateStatus.Rejected:
                    await _mediator.SendAsync(new RejectApprenticeshipChangeCommand
                    {
                        ApprenticeshipId = apprenticeshipId,
                        Caller = new Caller(providerId, CallerType.Provider),
                        UserId = submission.UserId,
                        UserName = submission.LastUpdatedByInfo?.Name
                    });
                    break;
                case Types.Apprenticeship.Types.ApprenticeshipUpdateStatus.Deleted:
                    await _mediator.SendAsync(new UndoApprenticeshipChangeCommand
                    {
                        ApprenticeshipId = apprenticeshipId,
                        Caller = new Caller(providerId, CallerType.Provider),
                        UserId = submission.UserId,
                        UserName = submission.LastUpdatedByInfo?.Name
                    });
                    break;
                default:
                    throw new InvalidOperationException($"Invalid update status {submission.UpdateStatus}");
            }

            _logger.Info($"Patched update for apprenticeship {apprenticeshipId} for provider {providerId} with status {submission.UpdateStatus}", providerId, apprenticeshipId: apprenticeshipId);
        }
                
        public async Task<long> PostBulkUploadFile(long providerId, BulkUploadFileRequest bulkUploadFile)
        {
            _logger.Trace($"Saving bulk upload file for provider {providerId} ", providerId: providerId);

            var result = await _mediator.SendAsync(
                new CreateBulkUploadCommand
                {
                    Caller = new Caller(providerId, CallerType.Provider),
                    ProviderId = providerId,
                    CommitmentId = bulkUploadFile.CommitmentId,
                    FileName = bulkUploadFile.FileName,
                    BulkUploadFile = bulkUploadFile.Data
                });

            _logger.Info($"Saved bulk upload for provider {providerId}", providerId: providerId);
            return result;
        }

        public async Task<string> GetBulkUploadFile(long providerId, long bulkUploadFileId)
        {
            _logger.Trace($"Getting bulk upload file for provider {providerId}, FileId {bulkUploadFileId} ", providerId: providerId);

            var result = await _mediator.SendAsync(
                new GetBulkUploadFileQuery
                {
                    Caller = new Caller(providerId, CallerType.Provider),
                    ProviderId = providerId, BulkUploadFileId = bulkUploadFileId
                });

            _logger.Info($"Retrieved bulk upload for provider {providerId}", providerId: providerId);
            return result.Data;
        }

        public async Task<GetProviderResponse> GetProvider(long providerId)
        {
            var result = await _mediator.SendAsync(new GetProviderQuery
            {
                Ukprn = providerId
            });
            
            return new GetProviderResponse
            {
                Provider = result.Provider
            }; 
        }
    }
}