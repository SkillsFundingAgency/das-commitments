using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Support.SubSite.Core.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.Core.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IValidator<ApprenticeshipSearchQuery> _searchValidator;
        private readonly IEncodingService _encodingService;

        public ApprenticeshipsOrchestrator(
                                            IMediator mediator,
                                            //IApprenticeshipMapper apprenticeshipMapper,
                                            IValidator<ApprenticeshipSearchQuery> searchValidator,
                                            IEncodingService encodingService)
        {
            _mediator = mediator;
            _searchValidator = searchValidator;
            _encodingService = encodingService;
        }

        public async Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId)
        {
            var apprenticeshipId = _encodingService.Decode(hashId, EncodingType.ApprenticeshipId);
            var accountId = _encodingService.Decode(accountHashedId, EncodingType.AccountId);

            var response = await _mediator.Send(new GetApprenticeshipQuery(apprenticeshipId));

            if (response == null)
            {
                var errorMsg = $"Can't find Apprenticeship with Hash Id {hashId}";

                throw new Exception(errorMsg);
            }

            return new ApprenticeshipViewModel
            {
                Uln = response.Uln,
                ApprenticeshipCode = response.CourseCode,
                DasTrainingStartDate = response.StartDate,
                DasTrainingEndDate = response.EndDate,
                DateOfBirth = response.DateOfBirth,
                Email = response.Email,
                EmployerReference = response.EmployerReference,
                FirstName = response.FirstName,
                LastName = response.LastName,
                Option = response.Option,
                TrainingProvider = response.ProviderName,
                UKPRN = response.ProviderId,
                Version = response.Version
            };
        }

        public async Task<UlnSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery)
        {
            long employerAccountId;
            try
            {
                employerAccountId = _encodingService.Decode(searchQuery.HashedAccountId, EncodingType.AccountId);
            }
            catch (Exception ex)
            {
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

            return new UlnSummaryViewModel
            {
                Uln = searchQuery.SearchTerm,
                ApprenticeshipsCount = response.TotalApprenticeshipsFound,
                CurrentHashedAccountId = searchQuery.HashedAccountId,
                SearchResults = response.Apprenticeships.Select(app =>
                {
                    return new ApprenticeshipSearchItemViewModel
                    {
                        ApprenticeName = app.FirstName + "" + app.LastName,
                        ApprenticeshipHashId = _encodingService.Encode(app.Id, EncodingType.ApprenticeshipId),
                        DateOfBirth = app.DateOfBirth,
                        EmployerName = app.EmployerName
                    };
                }).ToList()
            };
        }

        public async Task<CommitmentSummaryViewModel> GetCommitmentSummary(ApprenticeshipSearchQuery searchQuery)
        {
            long commitmentId = 0;
            long accountId = 0;

            try
            {
                commitmentId = _encodingService.Decode(searchQuery.SearchTerm, EncodingType.CohortReference);
            }
            catch (Exception ex)
            {
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Please enter a valid Cohort number" }
                };
            }

            try
            {
                accountId = _encodingService.Decode(searchQuery.HashedAccountId, EncodingType.AccountId);
            }
            catch (Exception ex)
            {
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "Problem validating your account Id" }
                };
            }

            var response = await _mediator.Send(new GetCohortSummaryQuery(commitmentId));

            if (response == null)
            {
                return new CommitmentSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            return new CommitmentSummaryViewModel
            {
                CohortReference = response.CohortReference,
                ProviderUkprn = response.ProviderId,
                HashedAccountId = searchQuery.HashedAccountId,
            };
        }

        public async Task<CommitmentDetailViewModel> GetCommitmentDetails(string hashCommitmentId)
        {
            long commitmentId = 0;

            try
            {
                commitmentId = _encodingService.Decode(hashCommitmentId, EncodingType.CohortReference);
            }
            catch (Exception ex)
            {
                throw;
            }

            var response = await _mediator.Send(new GetCohortSummaryQuery(commitmentId));

            if (response == null)
            {
                var errorMsg = $"Can't find Commitment with Hash Id {hashCommitmentId}";

                throw new Exception(errorMsg);
            }


            return new CommitmentDetailViewModel
            {
                CommitmentSummary = new CommitmentSummaryViewModel
                {
                    CohortReference = response.CohortReference,
                    ProviderUkprn = response.ProviderId,
                    HashedAccountId = _encodingService.Encode(response.AccountId, EncodingType.AccountId),
                },
                CommitmentApprenticeships = null
            };
        }
    }
}