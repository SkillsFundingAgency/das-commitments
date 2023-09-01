using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderUrlHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private readonly ILogger<BulkUploadValidateCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly EmployerSummaries _employerSummaries;
        private readonly IOverlapCheckService _overlapService;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private readonly IProviderRelationshipsApiClient _providerRelationshipsApiClient;
        private readonly IEmployerAgreementService _employerAgreementService;
        private readonly RplSettingsConfiguration _rplConfig;
        private List<BulkUploadAddDraftApprenticeshipRequest> _csvRecords;
        private Dictionary<string, Models.Cohort> _cachedCohortDetails;
        private readonly ILinkGenerator _urlHelper;
        private readonly IDbContextFactory _dbContextFactory;

        public long ProviderId { get; set; }
        public bool RplDataExtended { get; set; }
        public long? LogId { get; set; }

        public BulkUploadValidateCommandHandler(
            ILogger<BulkUploadValidateCommandHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IOverlapCheckService overlapService,
            IAcademicYearDateProvider academicYearDateProvider,
            IProviderRelationshipsApiClient providerRelationshipsApiClient,
            IEmployerAgreementService employerAgreementService,
            RplSettingsConfiguration rplConfig,
            ILinkGenerator urlHelper,
            IDbContextFactory dbContextFactory
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _employerSummaries = new EmployerSummaries();
            _overlapService = overlapService;
            _academicYearDateProvider = academicYearDateProvider;
            _providerRelationshipsApiClient = providerRelationshipsApiClient;
            _employerAgreementService = employerAgreementService;
            _rplConfig = rplConfig;
            _cachedCohortDetails = new Dictionary<string, Models.Cohort>();
            _urlHelper = urlHelper;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<BulkUploadValidateApiResponse> Handle(BulkUploadValidateCommand command, CancellationToken cancellationToken)
        {
            ProviderId = command.ProviderId;
            LogId = command.LogId;
            RplDataExtended = command.RplDataExtended;
            var bulkUploadValidationErrors = new List<BulkUploadValidationError>();
            _csvRecords = command.CsvRecords.ToList();


            var standardsError = ValidateHasDeclaredStandards(command.ProviderStandardResults, bulkUploadValidationErrors);

            if (standardsError.Any())
            {
                return new BulkUploadValidateApiResponse
                {
                    BulkUploadValidationErrors = standardsError
                };
            }

            foreach (var csvRecord in command.CsvRecords)
            {
                var criticalDomainError = await ValidateCriticalErrors(csvRecord, command.ProviderId);
                await AddError(bulkUploadValidationErrors, csvRecord, criticalDomainError);

                if (!criticalDomainError.Any())
                {
                    var domainErrors = await Validate(csvRecord, command.ProviderId, command.ReservationValidationResults, command.ProviderStandardResults);
                    await AddError(bulkUploadValidationErrors, csvRecord, domainErrors);
                }
            }

            await Log(command.LogId.Value, bulkUploadValidationErrors);

            return new BulkUploadValidateApiResponse
            {
                BulkUploadValidationErrors = bulkUploadValidationErrors
            };
        }

        private async Task Log(long? logId, List<BulkUploadValidationError> errors)
        {
            // Close db connection and create transaction else throw will dispose 
            var db = _dbContextFactory.CreateDbContext();

            var fileUploadLog = await db.FileUploadLogs.FirstOrDefaultAsync(a => a.Id == logId);

            if (fileUploadLog != null)
            {
                db.Database.CurrentTransaction?.Commit();
                var transaction = db.Database.BeginTransaction();
                fileUploadLog.Error = JsonConvert.SerializeObject(errors);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }

        private async Task AddError(List<BulkUploadValidationError> bulkUploadValidationErrors, BulkUploadAddDraftApprenticeshipRequest csvRecord, List<Error> domainErrors)
        {
            if (domainErrors.Any())

            {
                bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                    csvRecord.RowNumber,
                    await GetEmployerName(csvRecord.AgreementId),
                    csvRecord.Uln,
                    csvRecord.FirstName + " " + csvRecord.LastName,
                    domainErrors
                    ));
            }
        }

        private async Task<List<Error>> ValidateCriticalErrors(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId)
        {
            var domainErrors = await ValidateAgreementIdValidFormat(csvRecord);
            if (!domainErrors.Any())
            {
                domainErrors.AddRange(await ValidateAgreementIdIsSigned(csvRecord));

                // when a valid agreement has not been signed validation will stop
                if (domainErrors.Any())
                    return domainErrors;
            }

            var employerDetails = await GetEmployerDetails(csvRecord.AgreementId);
            if (((employerDetails.IsLevy.HasValue && !employerDetails.IsLevy.Value) || string.IsNullOrEmpty(csvRecord.CohortRef)) && !IsFundedByTransfer(csvRecord.CohortRef))
            {
                if (!await ValidatePermissionToCreateCohort(csvRecord, providerId, domainErrors, employerDetails.IsLevy))
                {
                    // when a provider doesn't have permission to create cohort or reserve funding (non-levy) - the validation will stop
                    return domainErrors;
                }
            }

            return domainErrors;
        }

        private List<BulkUploadValidationError> ValidateHasDeclaredStandards(ProviderStandardResults providerStandardResults, List<BulkUploadValidationError> bulkUploadValidationErrors)
        {
            var domainErrors = ValidateDeclaredStandards(providerStandardResults);

            if (domainErrors.Any())

            {
                bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                    0,
                    null,
                    null,
                    null,
                    domainErrors
                ));
            }

            return bulkUploadValidationErrors;
        }

        /// <summary>
        /// If it is funded by Transfer - non-levy employer doesn't need to check for the permission to create cohort.
        /// </summary>
        /// <param name="cohortRef"></param>
        /// <returns></returns>
        private bool IsFundedByTransfer(string cohortRef)
        {
            if (!string.IsNullOrWhiteSpace(cohortRef))
            {
                var cohortDetails = GetCohortDetails(cohortRef);

                if (cohortDetails.TransferSenderId.HasValue)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<List<Error>> Validate(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, BulkReservationValidationResults reservationValidationResults, ProviderStandardResults providerStandardResults)
        {
            var domainErrors = await ValidateAgreementIdValidFormat(csvRecord);
            if (!domainErrors.Any())
            {
                domainErrors.AddRange(await ValidateAgreementIdIsSigned(csvRecord));

                // when a valid agreement has not been signed validation will stop
                if (domainErrors.Any())
                    return domainErrors;
            }

            domainErrors.AddRange(await ValidateCohortRef(csvRecord, providerId));
            domainErrors.AddRange(ValidateUln(csvRecord));
            domainErrors.AddRange(ValidateFamilyName(csvRecord));
            domainErrors.AddRange(ValidateGivenName(csvRecord));
            domainErrors.AddRange(ValidateDateOfBirth(csvRecord));
            domainErrors.AddRange(ValidateEmailAddress(csvRecord));
            domainErrors.AddRange(ValidateCourseCode(csvRecord, providerStandardResults));
            domainErrors.AddRange(ValidateStartDate(csvRecord));
            domainErrors.AddRange(ValidateEndDate(csvRecord));
            domainErrors.AddRange(ValidateCost(csvRecord));
            domainErrors.AddRange(ValidateProviderRef(csvRecord));
            domainErrors.AddRange(ValidateEPAOrgId(csvRecord));
            domainErrors.AddRange(ValidateReservation(csvRecord, reservationValidationResults));

            if (!RplDataExtended)
            {
                domainErrors.AddRange(ValidatePriorLearning(csvRecord));
            }
            else
            {
                domainErrors.AddRange(ValidateRecognisePriorLearning(csvRecord));
                domainErrors.AddRange(ValidateTrainingTotalHours(csvRecord));
                domainErrors.AddRange(ValidateTrainingHoursReduction(csvRecord, _rplConfig.MaximumTrainingTimeReduction));
                domainErrors.AddRange(ValidateDurationReducedBy(csvRecord));
                domainErrors.AddRange(ValidatePriceReducedBy(csvRecord, _rplConfig.MinimumPriceReduction));
            }

            return domainErrors;
        }

        private async Task<string> GetEmployerName(string agreementId)
        {
            var employerDetails = await GetEmployerDetails(agreementId);
            return employerDetails.Name;
        }

        private async Task<EmployerSummary> GetEmployerDetails(string agreementId)
        {
            if (!string.IsNullOrEmpty(agreementId))
            {
                if (_employerSummaries.ContainsKey(agreementId))
                {
                    var result = _employerSummaries.GetValueOrDefault(agreementId);
                    return result;
                }

                var accountLegalEntity = _dbContext.Value.AccountLegalEntities
                  .Include(x => x.Account)
                  .Where(x => x.PublicHashedId == agreementId).FirstOrDefault();

                if (accountLegalEntity != null)
                {
                    var employerName = accountLegalEntity.Account.Name;
                    var isLevy = accountLegalEntity.Account.LevyStatus == Types.ApprenticeshipEmployerType.Levy;
                    var isSigned = await _employerAgreementService.IsAgreementSigned(accountLegalEntity.AccountId, accountLegalEntity.MaLegalEntityId);
                    var employerSummary = new EmployerSummary(agreementId, accountLegalEntity.Id, isLevy, employerName, isSigned, accountLegalEntity.LegalEntityId);
                    _employerSummaries.Add(employerSummary);
                    return employerSummary;
                }
            }

            return new EmployerSummary(agreementId, null, null, string.Empty, null, string.Empty);
        }

        private Models.Cohort GetCohortDetails(string cohortRef)
        {
            if (_cachedCohortDetails.ContainsKey(cohortRef))
            {
                return _cachedCohortDetails.GetValueOrDefault(cohortRef);
            }

            var cohort = _dbContext.Value.Cohorts
                .Include(x => x.AccountLegalEntity)
                .Include(x => x.Apprenticeships)
                .Where(x => x.Reference == cohortRef).FirstOrDefault();
            _cachedCohortDetails.Add(cohortRef, cohort);

            return cohort;
        }

        private Models.Standard GetStandardDetails(string stdCode)
        {
            if (!string.IsNullOrWhiteSpace(stdCode))
            {
                int.TryParse(stdCode, out int result);

                var standard = _dbContext.Value.Standards
                    .Where(x => x.LarsCode == result).FirstOrDefault();

                return standard;
            }

            return null;
        }
    }
}