using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.ProviderPayment;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using System.Collections.Generic;
using SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary;
using Originator = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.Originator;
using PaymentStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class EmployerOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        private readonly FacetMapper _facetMapper;

        private readonly ApprenticeshipFilterService _apprenticeshipFilterService;

        public EmployerOrchestrator(
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

        public async Task<GetCommitmentsResponse> GetCommitments(long accountId)
        {
            _logger.Trace($"Getting commitments for employer account {accountId}", accountId: accountId);

            var response = await _mediator.SendAsync(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });

            _logger.Info($"Retrieved commitments for employer account {accountId}. {response.Data?.Count} commitments found", accountId: accountId);

            return response;
        }

        public async Task<GetCommitmentResponse> GetCommitment(long accountId, long commitmentId)
        {
            _logger.Trace($"Getting commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            var response = await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId
            });

            _logger.Info($"Retrieved commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            return response;
        }

        public async Task<long> CreateCommitment(long accountId, Commitment.CommitmentRequest commitmentRequest)
        {
            _logger.Trace($"Creating commitment for employer account {accountId}", accountId: accountId);

            commitmentRequest.Commitment.EmployerAccountId = accountId;

            var id = await _mediator.SendAsync(new CreateCommitmentCommand
            {
                Commitment = commitmentRequest.Commitment,
                UserId = commitmentRequest.UserId,
                CallerType = CallerType.Employer,
                Message = commitmentRequest.Message
            });

            _logger.Info($"Created commitment {id} for employer account {accountId}", accountId: accountId);

            return id;
        }

        public async Task<GetApprenticeshipsResponse> GetApprenticeships(long accountId)
        {
            _logger.Trace($"Getting apprenticeships for employer account {accountId}", accountId: accountId);

            var response = await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });

            _logger.Info($"Retrieved apprenticeships for employer account {accountId}. {response.Data.Count} apprenticeships found", accountId: accountId);

            return response;
        }

        public async Task<Apprenticeship.ApprenticeshipSearchResponse> GetApprenticeships(long accountId, Apprenticeship.ApprenticeshipSearchQuery query)
        {
            _logger.Trace($"Getting apprenticeships with filter query for employer {accountId}. Page: {query.PageNumber}, PageSize: {query.PageSize}", accountId: accountId);

            var response = await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });

            var approvedApprenticeships = response.Data
                .Where(m => m.PaymentStatus != PaymentStatus.PendingApproval).ToList();

            var facets = _facetMapper.BuildFacets(approvedApprenticeships , query, Originator.Employer);

            var filteredApprenticeships = _apprenticeshipFilterService.Filter(approvedApprenticeships, query, Originator.Employer);

            _logger.Info($"Retrieved {approvedApprenticeships.Count} apprenticeships with filter query for employer {accountId}. Page: {query.PageNumber}, PageSize: {query.PageSize}", accountId: accountId);

            return new Apprenticeship.ApprenticeshipSearchResponse
            {
                Apprenticeships = filteredApprenticeships.Results,
                Facets = facets,
                TotalApprenticeships = approvedApprenticeships.Count,
                PageNumber = filteredApprenticeships.PageNumber,
                PageSize = filteredApprenticeships.PageSize
            };
        }

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long accountId, long apprenticeshipId)
        {
            _logger.Trace($"Getting apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);

            var response = await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                ApprenticeshipId = apprenticeshipId
            });

            if(response.Data != null)
                _logger.Info($"Retrieved apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId, commitmentId: response.Data.CommitmentId);
            else
                _logger.Info($"Couldn't find apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);

            return response;
        }

        public async Task<long> CreateApprenticeship(long accountId, long commitmentId, Apprenticeship.ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Trace($"Creating apprenticeship for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            var id = await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId,
                UserName = apprenticeshipRequest.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Created apprenticeship {id} for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId, apprenticeshipId: id);

            return id;
        }

        public async Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship.ApprenticeshipRequest apprenticeshipRequest)
        {
            _logger.Trace($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            apprenticeshipRequest.Apprenticeship.CommitmentId = commitmentId;

            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                Apprenticeship = apprenticeshipRequest.Apprenticeship,
                UserId = apprenticeshipRequest.UserId,
                UserName = apprenticeshipRequest.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Updated apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);
        }

        public async Task UpdateCustomProviderPaymentPriority(long accountId, ProviderPaymentPrioritySubmission submission)
        {
            _logger.Trace($"Updating Provider Payment Priority for employer account {accountId}", accountId);

            var response = await _mediator.SendAsync(new UpdateProviderPaymentsPriorityCommand
            {
                EmployerAccountId = accountId,
                ProviderPriorities = CreateListOfProviders(submission.Priorities)
            });

            _logger.Info($"Updated Provider Payment Priorities with {submission.Priorities.Count} providers for employer account {accountId}", accountId);
        }

        public async Task<GetProviderPaymentsPriorityResponse> GetCustomProviderPaymentPriority(long accountId)
        {
            _logger.Trace($"Getting Provider Payment Priority for employer account {accountId}", accountId);

            var response = await _mediator.SendAsync(new GetProviderPaymentsPriorityRequest
            {
                EmployerAccountId = accountId
            });

            _logger.Info($"Retrieved {response.Data.Count} Provider Payment Priorities for employer account {accountId}", accountId);

            return response;
        }

        public async Task PatchCommitment(long accountId, long commitmentId, CommitmentSubmission submission)
        {
            _logger.Trace($"Updating latest action to {submission.Action} for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                LatestAction = submission.Action,
                LastUpdatedByName = submission.LastUpdatedByInfo.Name,
                LastUpdatedByEmail = submission.LastUpdatedByInfo.EmailAddress,
                UserId = submission.UserId,
                Message = submission.Message
            });

            _logger.Info($"Updated latest action to {submission.Action} for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);
        }

        public async Task PatchApprenticeship(long accountId, long apprenticeshipId, Apprenticeship.ApprenticeshipSubmission apprenticeshipSubmission)
        {
            _logger.Trace($"Updating payment status to {apprenticeshipSubmission.PaymentStatus} for apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new UpdateApprenticeshipStatusCommand
            {
                AccountId = accountId,
                ApprenticeshipId = apprenticeshipId,
                PaymentStatus = apprenticeshipSubmission.PaymentStatus,
                DateOfChange = apprenticeshipSubmission.DateOfChange,
                UserId = apprenticeshipSubmission.UserId,
                UserName = apprenticeshipSubmission.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Updated payment status to {apprenticeshipSubmission.PaymentStatus} for apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);
        }

        public async Task DeleteApprenticeship(long accountId, long apprenticeshipId, string userId, string userName)
        {
            _logger.Trace($"Deleting apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new DeleteApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                ApprenticeshipId = apprenticeshipId,
                UserId = userId,
                UserName = userName
            });

            _logger.Info($"Deleted apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);
        }

        public async Task DeleteCommitment(long accountId, long commitmentId, string userId, string userName)
        {
            _logger.Trace($"Deleting commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            await _mediator.SendAsync(new DeleteCommitmentCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                UserId = userId,
                UserName = userName
            });

            _logger.Info($"Deleted commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);
        }

        public async Task<GetPendingApprenticeshipUpdateResponse> GetPendingApprenticeshipUpdate(long accountId, long apprenticeshipId)
        {
            _logger.Trace($"Getting pending update for apprenticeship {apprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: apprenticeshipId );

            var response = await _mediator.SendAsync(new GetPendingApprenticeshipUpdateRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                ApprenticeshipId = apprenticeshipId
            });

            _logger.Info($"Retrieved pending update for apprenticeship {apprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: apprenticeshipId);

            return response;
        }

        public async Task CreateApprenticeshipUpdate(long accountId, Apprenticeship.ApprenticeshipUpdateRequest updateRequest)
        {
            _logger.Trace($"Creating update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);

            await _mediator.SendAsync(new CreateApprenticeshipUpdateCommand
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Employer,
                        Id = accountId
                    },
                    ApprenticeshipUpdate = updateRequest.ApprenticeshipUpdate,
                    UserName = updateRequest.LastUpdatedByInfo?.Name,
                    UserId = updateRequest.UserId
            });

            _logger.Info($"Created update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);
        }

        public async Task PatchApprenticeshipUpdate(long accountId, long apprenticeshipId, Apprenticeship.ApprenticeshipUpdateSubmission submission)
        {
            _logger.Info($"Patching update for apprenticeship {apprenticeshipId} for employer account {accountId} with status {submission.UpdateStatus}", accountId, apprenticeshipId: apprenticeshipId);

            var command = 
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = apprenticeshipId,
                    Caller = new Caller(accountId, CallerType.Employer),
                    UserId = submission.UserId,
                    UpdateStatus = (ApprenticeshipUpdateStatus)submission.UpdateStatus,
                    UserName = submission.LastUpdatedByInfo?.Name
                };

            await _mediator.SendAsync(command);

            _logger.Info($"Patched update for apprenticeship {apprenticeshipId} for employer account {accountId} with status {submission.UpdateStatus}", accountId, apprenticeshipId: apprenticeshipId);
        }

        private List<Domain.Entities.ProviderPaymentPriorityUpdateItem> CreateListOfProviders(IList<Types.ProviderPayment.ProviderPaymentPriorityUpdateItem> priorities)
        {
            if (priorities == null)
                new List<long>(0);

            return priorities.Select(x => new Domain.Entities.ProviderPaymentPriorityUpdateItem
            {
                ProviderId = x.ProviderId,
                PriorityOrder = x.PriorityOrder
            }).ToList();
        }

        public async Task<GetEmployerAccountSummaryResponse> GetAccountSummary(long accountId)
        {
            _logger.Trace($"Getting account summary for employer account {accountId}", accountId: accountId);

            var response = await _mediator.SendAsync(new GetEmployerAccountSummaryRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });

            _logger.Info($"Retrieved {response.Data.Count()} account summary items for employer account {accountId}", accountId: accountId);

            return response;
        }
    }
}
