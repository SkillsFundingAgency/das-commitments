using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.HashingService;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;

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

            var response = await _mediator.Send(new GetSupportApprenticeshipQuery
            {
                ApprenticeshipId = apprenticeshipId
            });

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

            var response = await _mediator.Send(new GetSupportApprenticeshipQuery
            {
                Uln = searchQuery.SearchTerm
            });

            if ((response?.Apprenticeships?.Count ?? 0) == 0)
            {
                return new UlnSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            _logger.LogInformation($"Apprenticeships Record Count: {response.Apprenticeships.Count}");

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
                var cohort = await _mediator.Send(new GetSupportCohortSummaryQuery(commitmentId));

                if (cohort == null)
                {
                    return new CommitmentSummaryViewModel
                    {
                        ReponseMessages = { "No record Found" }
                    };
                }

                _logger.LogInformation($"Commitment Record with Id: {cohort.CohortId}");

                var cohortApprenticeshipsResponse = await _mediator.Send(new GetSupportApprenticeshipQuery
                {
                    CohortId = cohort.CohortId
                });

                return _commitmentMapper.MapToCommitmentSummaryViewModel(cohort, cohortApprenticeshipsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Unable to load resource error" }
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

            var cohort = await _mediator.Send(new GetSupportCohortSummaryQuery(commitmentId));

            if (cohort == null)
            {
                var errorMsg = $"Can't find Commitment with Hash Id {hashCommitmentId}";
                _logger.LogWarning(errorMsg);

                throw new Exception(errorMsg);
            }

            var cohortApprenticeshipsResponse = await _mediator.Send(new GetSupportApprenticeshipQuery
            {
                CohortId = cohort.CohortId
            });

            return _commitmentMapper.MapToCommitmentDetailViewModel(cohort, cohortApprenticeshipsResponse);
        }
    }
}