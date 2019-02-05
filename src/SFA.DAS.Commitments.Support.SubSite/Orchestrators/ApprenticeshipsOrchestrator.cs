using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly ILog _logger;
        private readonly IMediator _mediator;
        private readonly IValidator<ApprenticeshipSearchQuery> _searchValidator;
        private readonly IHashingService _hashingService;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;
        private readonly ICommitmentMapper _commitmentMapper;

        public ApprenticeshipsOrchestrator(ILog logger,
                                            IMediator mediator,
                                            IApprenticeshipMapper apprenticeshipMapper,
                                            IValidator<ApprenticeshipSearchQuery> searchValidator,
                                            IHashingService hashingService,
                                            ICommitmentMapper commitmentMapper)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentException(nameof(mediator));
            _searchValidator = searchValidator ?? throw new ArgumentException(nameof(searchValidator));
            _hashingService = hashingService ?? throw new ArgumentException(nameof(hashingService));
            _apprenticeshipMapper = apprenticeshipMapper ?? throw new ArgumentException(nameof(apprenticeshipMapper));
            _commitmentMapper = commitmentMapper ?? throw new ArgumentException(nameof(commitmentMapper));
        }

        public async Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId)
        {
            _logger.Trace("Retrieving Apprenticeship Details");

            var apprenticeshipId = _hashingService.DecodeValue(hashId);
            var accountId = _hashingService.DecodeValue(accountHashedId);

            var response = await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    Id = accountId,
                    CallerType = CallerType.Support
                },
                ApprenticeshipId = apprenticeshipId
            });

            if (response == null)
            {
                var errorMsg = $"Can't find Apprenticeship with Hash Id {hashId}";
                _logger.Warn(errorMsg);

                throw new Exception(errorMsg);
            }

            return _apprenticeshipMapper.MapToApprenticeshipViewModel(response.Data);
        }

        public async Task<UlnSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery)
        {
            _logger.Trace("Retrieving Apprenticeships Record");

            var validationResult = _searchValidator.Validate(searchQuery);

            if (!validationResult.IsValid)
            {
                return new UlnSummaryViewModel
                {
                    ReponseMessages = validationResult.Errors.Select(o => o.ErrorMessage).ToList()
                };
            }

            var response = await _mediator.SendAsync(new GetApprenticeshipsByUlnRequest
            {
                Uln = searchQuery.SearchTerm
            });

            if ((response?.TotalCount ?? 0) == 0)
            {
                return new UlnSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            _logger.Info($"Apprenticeships Record Count: {response.TotalCount}");

            return _apprenticeshipMapper.MapToUlnResultView(response);
        }

        public async Task<CommitmentSummaryViewModel> GetCommitmentSummary(ApprenticeshipSearchQuery searchQuery)
        {
            _logger.Trace("Retrieving Commitment Details");

            var validationResult = _searchValidator.Validate(searchQuery);
            if (!validationResult.IsValid)
            {
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = validationResult.Errors.Select(o => o.ErrorMessage).ToList()
                };
            }

            long commitmentId = 0;
            long accountId = 0;

            try
            {
                commitmentId = _hashingService.DecodeValue(searchQuery.SearchTerm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to decode Hashed Commitment Id");

                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Please enter a valid Cohort number" }
                };
            }

            try
            {
                accountId = _hashingService.DecodeValue(searchQuery.HashedAccountId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to decode Hashed Account Id");

                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Problem validating your account Id" }
                };
            }

            var response = await _mediator.SendAsync(new GetAccountCommitmentRequest
            {
                CommitmentId = commitmentId,
                Caller = new Caller
                {
                    Id = accountId,
                    CallerType = CallerType.Support
                }
            });

            if (response?.Data == null)
            {
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            _logger.Info($"Commitment Record with Id: {response.Data.Id}");

            return _commitmentMapper.MapToCommitmentSummaryViewModel(response?.Data);
        }

        public async Task<CommitmentDetailViewModel> GetCommitmentDetails(string hashCommitmentId)
        {
            _logger.Trace("Retrieving Commitment Details");

            long commitmentId = 0;

            try
            {
                commitmentId = _hashingService.DecodeValue(hashCommitmentId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to decode Hashed Commitment Id");
                throw;
            }

            var response = await _mediator.SendAsync(new GetCommitmentRequest
            {
                CommitmentId = commitmentId,
                Caller = new Caller
                {
                    CallerType = CallerType.Support
                }
            });

            if (response?.Data == null)
            {
                var errorMsg = $"Can't find Commitment with Hash Id {hashCommitmentId}";
                _logger.Warn(errorMsg);

                throw new Exception(errorMsg);
            }

            return _commitmentMapper.MapToCommitmentDetailViewModel(response.Data);
        }
    }
}