using FluentValidation;
using MediatR;

//using SFA.DAS.Commitments.Application.Exceptions;
//using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
//using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
//using SFA.DAS.Commitments.Application.Queries.GetCommitment;
//using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.HashingService;

//using SFA.DAS.NLog.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;

//using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly ILogger<ApprenticeshipsOrchestrator> _logger;
        private readonly IMediator _mediator;
        private readonly IValidator<ApprenticeshipSearchQuery> _searchValidator;
        private readonly IHashingService _hashingService;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;
        private readonly ICommitmentMapper _commitmentMapper;

        public ApprenticeshipsOrchestrator(ILogger<ApprenticeshipsOrchestrator> logger,
                                            IMediator mediator,
                                            IApprenticeshipMapper apprenticeshipMapper,
                                            IValidator<ApprenticeshipSearchQuery> searchValidator,
                                            IHashingService hashingService,
                                            ICommitmentMapper commitmentMapper)
        {
            _logger = logger;
            _mediator = mediator;
            _searchValidator = searchValidator;
            _hashingService = hashingService;
            _apprenticeshipMapper = apprenticeshipMapper;
            _commitmentMapper = commitmentMapper;
        }

        public async Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId)
        {
            _logger.LogInformation("Retrieving Apprenticeship Details");

            var apprenticeshipId = _hashingService.DecodeValue(hashId);
            var accountId = _hashingService.DecodeValue(accountHashedId);

            var response = await _mediator.Send(new GetApprenticeshipQuery(apprenticeshipId));

            if (response == null)
            {
                var errorMsg = $"Can't find Apprenticeship with Hash Id {hashId}";
                _logger.LogWarning(errorMsg);

                throw new Exception(errorMsg);
            }

            return _apprenticeshipMapper.MapToApprenticeshipViewModel(response);
        }

        public async Task<UlnSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery)
        {
            _logger.LogInformation("Retrieving Apprenticeships Record");

            var validationResult = _searchValidator.Validate(searchQuery);

            if (!validationResult.IsValid)
            {
                return new UlnSummaryViewModel
                {
                    ReponseMessages = validationResult.Errors.Select(o => o.ErrorMessage).ToList()
                };
            }

            long employerAccountId;
            try
            {
                employerAccountId = _hashingService.DecodeValue(searchQuery.HashedAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to decode Hashed Employer Account Id");

                return new UlnSummaryViewModel
                {
                    ReponseMessages = { "Unable to decode the account id" }
                };
            }

            var filterValues = new ApprenticeshipSearchFilters
            {
                SearchTerm = searchQuery.SearchTerm,
            };

            var response = await _mediator.Send(new GetApprenticeshipsQuery
            {
                SearchFilters = filterValues,
                EmployerAccountId = employerAccountId
            });

            if ((response?.TotalApprenticeshipsFound ?? 0) == 0)
            {
                return new UlnSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            _logger.LogInformation($"Apprenticeships Record Count: {response?.TotalApprenticeshipsFound}");

            return _apprenticeshipMapper.MapToUlnResultView(response);
        }

        public async Task<CommitmentSummaryViewModel> GetCommitmentSummary(ApprenticeshipSearchQuery searchQuery)
        {
            _logger.LogInformation("Retrieving Commitment Details");

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
                _logger.LogError(ex, "Unable to decode Hashed Commitment Id");

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
                _logger.LogError(ex, "Unable to decode Hashed Account Id");

                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Problem validating your account Id" }
                };
            }

            try
            {
                var response = await _mediator.Send(new GetCohortSummaryQuery(commitmentId));

                if (response == null)
                {
                    return new CommitmentSummaryViewModel
                    {
                        ReponseMessages = { "No record Found" }
                    };
                }

                _logger.LogInformation($"Commitment Record with Id: {response.CohortId}");

                return _commitmentMapper.MapToCommitmentSummaryViewModel(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Account is unauthorised to access this Cohort." }
                };
            }
        }

        public async Task<CommitmentDetailViewModel> GetCommitmentDetails(string hashCommitmentId)
        {
            _logger.LogInformation("Retrieving Commitment Details");

            long commitmentId = 0;

            try
            {
                commitmentId = _hashingService.DecodeValue(hashCommitmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to decode Hashed Commitment Id");
                throw;
            }

            var response = await _mediator.Send(new GetCohortSummaryQuery(commitmentId));

            if (response == null)
            {
                var errorMsg = $"Can't find Commitment with Hash Id {hashCommitmentId}";
                _logger.LogWarning(errorMsg);

                throw new Exception(errorMsg);
            }

            return _commitmentMapper.MapToCommitmentDetailViewModel(response.Data);
        }
    }
}