using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using System.Threading;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly ILogger<ApprenticeshipsOrchestrator> _logger;
        private readonly IMediator _mediator;
        private readonly IValidator<ApprenticeshipSearchQuery> _searchValidator;
        private readonly IEncodingService _encodingService;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;
        private readonly ICommitmentMapper _commitmentMapper;

        public ApprenticeshipsOrchestrator(ILogger<ApprenticeshipsOrchestrator> logger,
            IMediator mediator,
            IApprenticeshipMapper apprenticeshipMapper,
            IValidator<ApprenticeshipSearchQuery> searchValidator,
            IEncodingService encodingService,
            ICommitmentMapper commitmentMapper)
        {
            _logger = logger;
            _mediator = mediator;
            _searchValidator = searchValidator;
            _encodingService = encodingService;
            _apprenticeshipMapper = apprenticeshipMapper;
            _commitmentMapper = commitmentMapper;
        }

        public async Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId)
        {
            _logger.LogInformation("Retrieving Apprenticeship Details");

            var apprenticeshipId = _encodingService.Decode(hashId, EncodingType.ApprenticeshipId);
            _ = _encodingService.Decode(accountHashedId, EncodingType.AccountId);

            var response = await _mediator.Send(new GetSupportApprenticeshipQuery
            {
                ApprenticeshipId = apprenticeshipId
            });

            var apprenticeshipUpdate = await _mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId,
                CommitmentsV2.Types.ApprenticeshipUpdateStatus.Pending));

            if (response == null)
            {
                var errorMsg = $"Can't find Apprenticeship with Hash Id {hashId}";
                _logger.LogWarning(errorMsg);

                throw new Exception(errorMsg);
            }

            var apprenticeshipProviders = await _mediator.Send(new GetChangeOfProviderChainQuery(apprenticeshipId), CancellationToken.None);

            var result = _apprenticeshipMapper.MapToApprenticeshipViewModel(response, apprenticeshipProviders);
            result.ApprenticeshipUpdates = _apprenticeshipMapper.MapToUpdateApprenticeshipViewModel(apprenticeshipUpdate, response.Apprenticeships.First());

            var overlappingTrainingDateRequest = (await _mediator.Send(new GetOverlappingTrainingDateRequestQuery(apprenticeshipId), CancellationToken.None))?.OverlappingTrainingDateRequests?.Where(x => x.Status == CommitmentsV2.Types.OverlappingTrainingDateRequestStatus.Pending)?.FirstOrDefault();
            result.OverlappingTrainingDateRequest = _apprenticeshipMapper.MapToOverlappingTrainingDateRequest(overlappingTrainingDateRequest);

            var priceEpisodesResult = await _mediator.Send(new GetPriceEpisodesQuery(apprenticeshipId));
            if (priceEpisodesResult.PriceEpisodes != null && priceEpisodesResult.PriceEpisodes.Any())
            {
                result.TrainingCost = priceEpisodesResult.PriceEpisodes.GetPrice();
            }

            return result;
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

            try
            {
                _ = _encodingService.Decode(searchQuery.HashedAccountId, EncodingType.AccountId);
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
            long accountId = 0;

            try
            {
                accountId = _encodingService.Decode(searchQuery.HashedAccountId, EncodingType.AccountId);
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
                commitmentId = _encodingService.Decode(searchQuery.SearchTerm, EncodingType.CohortReference);
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
                var cohort = await _mediator.Send(new GetSupportCohortSummaryQuery(commitmentId, accountId));

                if (cohort == null)
                {
                    return new CommitmentSummaryViewModel
                    {
                        ReponseMessages = { "No record Found" }
                    };
                }

                if (!cohort.AccountId.Equals(accountId))
                {
                    _logger.LogWarning($"Unauthorised to access Cohort {cohort.CohortId}");
                    return new CommitmentSummaryViewModel
                    {
                        ReponseMessages = { "Account is unauthorised to access this Cohort." }
                    };
                }

                _logger.LogInformation($"Commitment Record with Id: {cohort.CohortId}");

                var cohortApprenticeshipsResponse = await _mediator.Send(new GetSupportApprenticeshipQuery
                {
                    CohortId = cohort.CohortId,
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

        public async Task<CommitmentDetailViewModel> GetCommitmentDetails(string hashCommitmentId, string accountHashedId)
        {
            _logger.LogInformation("Retrieving Commitment Details");

            long commitmentId = 0;
            long accountId;

            try
            {
                commitmentId = _encodingService.Decode(hashCommitmentId, EncodingType.CohortReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to decode Hashed Commitment Id");
                throw;
            }

            try
            {
                accountId = _encodingService.Decode(accountHashedId, EncodingType.AccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to decode Hashed Account Id");
                throw;
            }

            var cohort = await _mediator.Send(new GetSupportCohortSummaryQuery(commitmentId, accountId));

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