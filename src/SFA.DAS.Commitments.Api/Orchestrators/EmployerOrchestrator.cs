using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment;
using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest;
using SFA.DAS.Commitments.Application.Commands.RejectTransferRequest;
using SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln;
using SFA.DAS.Commitments.Application.Queries.GetEmployerAccountIds;
using SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequest;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.HashingService;
using ApprenticeshipStatusSummary = SFA.DAS.Commitments.Domain.Entities.ApprenticeshipStatusSummary;
using Originator = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.Originator;
using PaymentStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.PaymentStatus;
using TransferApprovalStatus = SFA.DAS.Commitments.Api.Types.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class EmployerOrchestrator : IEmployerOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;
        private readonly ICommitmentMapper _commitmentMapper;
        private readonly ITransferRequestMapper _transferRequestMapper;
        private readonly IHashingService _hashingService;
        private readonly FacetMapper _facetMapper;
        private readonly ApprenticeshipFilterService _apprenticeshipFilterService;

        public EmployerOrchestrator(
            IMediator mediator, 
            ICommitmentsLogger logger,
            FacetMapper facetMapper,
            ApprenticeshipFilterService apprenticeshipFilterService,
            IApprenticeshipMapper apprenticeshipMapper,
            ICommitmentMapper commitmentMapper,
            ITransferRequestMapper transferRequestMapper,
            IHashingService hashingService)
        {
            _mediator = mediator;
            _logger = logger;
            _facetMapper = facetMapper;
            _apprenticeshipFilterService = apprenticeshipFilterService;
            _apprenticeshipMapper = apprenticeshipMapper;
            _commitmentMapper = commitmentMapper;
            _transferRequestMapper = transferRequestMapper;
            _hashingService = hashingService;
        }

        public async Task<IEnumerable<Commitment.CommitmentListItem>> GetCommitments(long accountId)
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

            return _commitmentMapper.MapFrom(response.Data, CallerType.Employer);
        }

        public Task<Commitment.CommitmentView> GetCommitment(long accountId, long commitmentId) =>
            GetCommitment(accountId, commitmentId, CallerType.Employer);

        public async Task<Commitment.CommitmentView> GetCommitment(long accountId, long commitmentId, CallerType callerType)
        {
            _logger.Trace($"Getting commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            var response = await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = callerType,
                    Id = accountId
                },
                CommitmentId = commitmentId
            });

            _logger.Info($"Retrieved commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            return _commitmentMapper.MapFrom(response.Data, CallerType.Employer);
        }

        public async Task<IEnumerable<Apprenticeship.Apprenticeship>> GetApprenticeships(long accountId)
        {
            _logger.Trace($"Getting apprenticeships for employer account {accountId}", accountId: accountId);

            var response = await _mediator.SendAsync(new GetApprenticeshipsRequest
            {
                Caller = new Caller { CallerType = CallerType.Employer, Id = accountId }
            });

            _logger.Info($"Retrieved apprenticeships for employer account {accountId}. {response.Apprenticeships.Count} apprenticeships found", accountId: accountId);

            return _apprenticeshipMapper.MapFromV2(response.Apprenticeships, CallerType.Employer);
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

            var apiApprenticeships = _apprenticeshipMapper.MapFromV2(response.Apprenticeships, CallerType.Employer).ToList();

            var totalApprenticeshipsBeforeFilter = response.TotalCount - apiApprenticeships.Count(m => m.PaymentStatus == PaymentStatus.PendingApproval);

            var approvedApprenticeships = apiApprenticeships
                .Where(m => m.PaymentStatus != PaymentStatus.PendingApproval).ToList();

            _logger.Info($"Searching for {query.SearchKeyword} by Employer {accountId}", accountId: accountId);

            var facets = _facetMapper.BuildFacets(approvedApprenticeships, query, Originator.Employer);
            var filteredApprenticeships = _apprenticeshipFilterService.Filter(approvedApprenticeships, query, Originator.Employer);

            _logger.Info($"Retrieved {approvedApprenticeships.Count} apprenticeships with filter query for employer {accountId}. Page: {query.PageNumber}, PageSize: {query.PageSize}", accountId: accountId);

            return new Apprenticeship.ApprenticeshipSearchResponse
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

        public async Task<Apprenticeship.Apprenticeship> GetApprenticeship(long accountId, long apprenticeshipId)
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

            if (response.Data == null)
            {
                _logger.Info($"Couldn't find apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);
                return null;
            }

            _logger.Info($"Retrieved apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId, commitmentId: response.Data.CommitmentId);
           
            return _apprenticeshipMapper.MapFrom(response.Data, CallerType.Employer);
        }

        public async Task PutApprenticeshipStopDate(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship.ApprenticeshipStopDate stopDate)
        {
            _logger.Trace($"Updating stop date to {stopDate.NewStopDate} for apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);

            await _mediator.SendAsync(new UpdateApprenticeshipStopDateCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                AccountId = accountId,
                ApprenticeshipId = apprenticeshipId,
                StopDate = stopDate.NewStopDate,
                UserId = stopDate.UserId,
                UserName = stopDate.LastUpdatedByInfo?.Name
            });

            _logger.Info($"Updated stop date to {stopDate.NewStopDate} for  apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId, apprenticeshipId: apprenticeshipId);
        }

        public async Task SetTransferApprovalStatus(long transferSenderId, long commitmentId, long transferRequestId, Commitment.TransferApprovalRequest transferApprovalRequest)
        {
            _logger.Trace($"Setting Approval status on commitment {commitmentId} for transfer sender employer account {transferSenderId}", accountId: transferSenderId, commitmentId: commitmentId);

            if (transferApprovalRequest.TransferApprovalStatus == TransferApprovalStatus.Approved)
            {
                await _mediator.SendAsync(new ApproveTransferRequestCommand
                {
                    TransferSenderId = transferSenderId,
                    TransferReceiverId = transferApprovalRequest.TransferReceiverId,
                    TransferRequestId = transferRequestId,
                    CommitmentId = commitmentId,
                    UserId = transferApprovalRequest.UserId,
                    UserEmail = transferApprovalRequest.UserEmail,
                    UserName = transferApprovalRequest.UserName
                });
            }
            else if (transferApprovalRequest.TransferApprovalStatus == TransferApprovalStatus.Rejected)
            {
                await _mediator.SendAsync(new RejectTransferRequestCommand
                {
                    TransferSenderId = transferSenderId,
                    TransferReceiverId = transferApprovalRequest.TransferReceiverId,
                    TransferRequestId = transferRequestId,
                    CommitmentId = commitmentId,
                    UserId = transferApprovalRequest.UserId,
                    UserEmail = transferApprovalRequest.UserEmail,
                    UserName = transferApprovalRequest.UserName
                });
            }
            else
            {
                throw new ArgumentException($"Invalid Transfer Approval Status of {transferApprovalRequest.TransferApprovalStatus}");
            }          

            _logger.Info($"Setting Approval Status for commitment {commitmentId} for transfer sender employer account {transferSenderId}", accountId: transferSenderId, commitmentId: commitmentId);
        }

        public async Task<Types.Commitment.TransferRequest> GetTransferRequest(long transferRequestId, long accountId, CallerType callerType)
        {
            _logger.Trace($"Getting transfer request {transferRequestId} for caller type {callerType.ToString()}", accountId: accountId);

            var response = await _mediator.SendAsync(new GetTransferRequestRequest
            {
                Caller = new Caller(accountId, callerType),
                TransferRequestId = transferRequestId
            });

            _logger.Info($"Retrieved transfer request {transferRequestId}", accountId: accountId);

            return _transferRequestMapper.MapFrom(response.Data);
        }

        public async Task<IList<Types.Commitment.TransferRequestSummary>> GetTransferRequests(string hashedAccountId)
        {
            var receiverRequests = await GetTransferRequestsForReceiver(hashedAccountId);
            var senderTransfers = await GetTransferRequestsForSender(hashedAccountId);

            return receiverRequests.Concat(senderTransfers).ToList();
        }

        private async Task<IList<Types.Commitment.TransferRequestSummary>> GetTransferRequestsForSender(string hashedTransferSenderId)
        {
            var transferSenderId = _hashingService.DecodeValue(hashedTransferSenderId);

            _logger.Trace($"Getting transfer requests employer sender account {transferSenderId}", accountId: transferSenderId);

            var response = await _mediator.SendAsync(new GetTransferRequestsForSenderRequest
            {
                TransferSenderAccountId = transferSenderId
            });

            _logger.Info($"Retrieved transfer requests for employer sender account {transferSenderId}. {response.Data.Count} transfer requests found", accountId: transferSenderId);

            return _transferRequestMapper.MapFrom(response.Data, Commitment.TransferType.AsSender).ToList();
        }

        private async Task<IList<Types.Commitment.TransferRequestSummary>> GetTransferRequestsForReceiver(string hashedtransferReceiverId)
        {
            var transferReceiverId = _hashingService.DecodeValue(hashedtransferReceiverId);

            _logger.Trace($"Getting transfer requests employer receiver account {transferReceiverId}", accountId: transferReceiverId);

            var response = await _mediator.SendAsync(new GetTransferRequestsForReceiverRequest
            {
                TransferReceiverAccountId = transferReceiverId
            });

            _logger.Info($"Retrieved transfer requests for employer receiver account {transferReceiverId}. {response.Data.Count} transfer requests found", accountId: transferReceiverId);

            return _transferRequestMapper.MapFrom(response.Data, Commitment.TransferType.AsReceiver).ToList();
        }

        public async Task PatchCommitment(long accountId, long commitmentId, CommitmentSubmission submission)
        {
            _logger.Trace($"Updating latest action to {submission.Action} for commitment {commitmentId} for employer account {accountId}", accountId: accountId, commitmentId: commitmentId);

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller { CallerType = CallerType.Employer, Id = accountId },
                CommitmentId = commitmentId,
                LatestAction = (LastAction)submission.Action,
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

            switch (apprenticeshipSubmission.PaymentStatus)
            {
                case PaymentStatus.Active:
                    await IssueResumeCommand(accountId, apprenticeshipId, apprenticeshipSubmission);
                    break;
                case PaymentStatus.Paused:
                    await IssuePauseCommand(accountId, apprenticeshipId, apprenticeshipSubmission);
                    break;
                case PaymentStatus.Withdrawn:
                    await IssueStopCommand(accountId, apprenticeshipId, apprenticeshipSubmission);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(apprenticeshipSubmission.PaymentStatus), "Not a valid value for change of status");
            }
            _logger.Info($"Updated payment status to {apprenticeshipSubmission.PaymentStatus} for apprenticeship {apprenticeshipId} for employer account {accountId}", accountId: accountId, apprenticeshipId: apprenticeshipId);
        }

        private async Task IssueStopCommand(long accountId, long apprenticeshipId, Apprenticeship.ApprenticeshipSubmission apprenticeshipSubmission)
        {
            await _mediator.SendAsync(new StopApprenticeshipCommand
            {
                Caller = new Caller(accountId, CallerType.Employer),
                AccountId = accountId,
                ApprenticeshipId = apprenticeshipId,
                DateOfChange = apprenticeshipSubmission.DateOfChange,
                UserId = apprenticeshipSubmission.UserId,
                UserName = apprenticeshipSubmission.LastUpdatedByInfo?.Name,
                MadeRedundant = apprenticeshipSubmission.MadeRedundant
            });
        }

        private async Task IssuePauseCommand(long accountId, long apprenticeshipId, Apprenticeship.ApprenticeshipSubmission apprenticeshipSubmission)
        {
            await _mediator.SendAsync(new PauseApprenticeshipCommand
            {
                Caller = new Caller(accountId, CallerType.Employer),
                AccountId = accountId,
                ApprenticeshipId = apprenticeshipId,
                DateOfChange = apprenticeshipSubmission.DateOfChange,
                UserId = apprenticeshipSubmission.UserId,
                UserName = apprenticeshipSubmission.LastUpdatedByInfo?.Name
            });
        }

        private async Task IssueResumeCommand(long accountId, long apprenticeshipId,
            Apprenticeship.ApprenticeshipSubmission apprenticeshipSubmission)
        {
            await _mediator.SendAsync(new ResumeApprenticeshipCommand
            {
                Caller = new Caller(accountId, CallerType.Employer),
                AccountId = accountId,
                ApprenticeshipId = apprenticeshipId,
                DateOfChange = apprenticeshipSubmission.DateOfChange,
                UserId = apprenticeshipSubmission.UserId,
                UserName = apprenticeshipSubmission.LastUpdatedByInfo?.Name
            });
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

        public async Task<IEnumerable<Apprenticeship.Apprenticeship>> GetActiveApprenticeshipsForUln(long accountId, string uln)
        {
            _logger.Trace($"Getting active apprenticeships for employer account {accountId}", accountId);

            var response = await _mediator.SendAsync(new GetActiveApprenticeshipsByUlnRequest
            {
                Uln = uln
            });

            _logger.Info($"Retrieved active apprenticeships for employer account {accountId}", accountId);

            return _apprenticeshipMapper.MapFrom(response.Data);
        }

        public async Task<Apprenticeship.ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long accountId, long apprenticeshipId)
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

            return _apprenticeshipMapper.MapApprenticeshipUpdate(response?.Data);
            
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
                    ApprenticeshipUpdate = _apprenticeshipMapper.MapApprenticeshipUpdate(updateRequest.ApprenticeshipUpdate),
                    UserName = updateRequest.LastUpdatedByInfo?.Name,
                    UserId = updateRequest.UserId
            });


            _logger.Info($"Created update for apprenticeship {updateRequest.ApprenticeshipUpdate.ApprenticeshipId} for employer account {accountId}", accountId, apprenticeshipId: updateRequest.ApprenticeshipUpdate.ApprenticeshipId);
        }

        public async Task PatchApprenticeshipUpdate(long accountId, long apprenticeshipId, Apprenticeship.ApprenticeshipUpdateSubmission submission)
        {
            _logger.Info($"Patching update for apprenticeship {apprenticeshipId} for employer account {accountId} with status {submission.UpdateStatus}", accountId, apprenticeshipId: apprenticeshipId);

            switch (submission.UpdateStatus)
            {
                case Apprenticeship.Types.ApprenticeshipUpdateStatus.Approved:
                    await _mediator.SendAsync(new AcceptApprenticeshipChangeCommand
                    {
                        ApprenticeshipId = apprenticeshipId,
                        Caller = new Caller(accountId, CallerType.Employer),
                        UserId = submission.UserId,
                        UserName = submission.LastUpdatedByInfo?.Name
                    });
                    break;
                case Apprenticeship.Types.ApprenticeshipUpdateStatus.Rejected:
                    await _mediator.SendAsync(new RejectApprenticeshipChangeCommand
                    {
                        ApprenticeshipId = apprenticeshipId,
                        Caller = new Caller(accountId, CallerType.Employer),
                        UserId = submission.UserId,
                        UserName = submission.LastUpdatedByInfo?.Name
                    });
                    break;
                case Apprenticeship.Types.ApprenticeshipUpdateStatus.Deleted:
                    await _mediator.SendAsync(new UndoApprenticeshipChangeCommand
                    {
                        ApprenticeshipId = apprenticeshipId,
                        Caller = new Caller(accountId, CallerType.Employer),
                        UserId = submission.UserId,
                        UserName = submission.LastUpdatedByInfo?.Name
                    });
                    break;
                default:
                    throw new InvalidOperationException($"Invalid update status {submission.UpdateStatus}");
            }

            _logger.Info($"Patched update for apprenticeship {apprenticeshipId} for employer account {accountId} with status {submission.UpdateStatus}", accountId, apprenticeshipId: apprenticeshipId);
        }

        public async Task<IEnumerable<Types.ApprenticeshipStatusSummary>> GetAccountSummary(long accountId)
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

            return Map(response.Data);
        }
        public async Task<IEnumerable<long>> GetEmployerAccountIds()
        {
            _logger.Trace("Getting all employer account ids");

            var response = await _mediator.SendAsync(new GetEmployerAccountIdsRequest());

            _logger.Info($"Retrieved {response.Data.Count()} account Ids");

            return response.Data;
        }

        private IEnumerable<Types.ApprenticeshipStatusSummary> Map(IEnumerable<ApprenticeshipStatusSummary> data)
        {
            return data.Select(s => new Types.ApprenticeshipStatusSummary
            {
                LegalEntityIdentifier = s.LegalEntityIdentifier,
                LegalEntityOrganisationType = s.LegalEntityOrganisationType,
                PendingApprovalCount = s.PendingApprovalCount,
                ActiveCount = s.ActiveCount,
                PausedCount = s.PausedCount,
                WithdrawnCount = s.WithdrawnCount,
                CompletedCount = s.CompletedCount
            });
        }
    }
}
