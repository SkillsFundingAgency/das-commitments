using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private readonly ILogger<BulkUploadValidateCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly Dictionary<string, (string Name, bool? IsLevy, bool? IsSigned)> _employerNames;
        private readonly IOverlapCheckService _overlapService;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private readonly IEmployerAgreementService _employerAgreementService;
        private List<CsvRecord> _csvRecords;

        public long ProviderId { get; set; }

        public BulkUploadValidateCommandHandler(
            ILogger<BulkUploadValidateCommandHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IOverlapCheckService overlapService,
            IAcademicYearDateProvider academicYearDateProvider,
            IEmployerAgreementService employerAgreementService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _employerNames = new Dictionary<string, (string Name, bool? IsLevy, bool? IsSigned)>();
            _overlapService = overlapService;
            _academicYearDateProvider = academicYearDateProvider;
            _employerAgreementService = employerAgreementService;
        }

        public async Task<BulkUploadValidateApiResponse> Handle(BulkUploadValidateCommand command, CancellationToken cancellationToken)
        {
            ProviderId = command.ProviderId;
            var bulkUploadValidationErrors = new List<BulkUploadValidationError>();
            _csvRecords = command.CsvRecords.ToList();
            foreach (var csvRecord in command.CsvRecords)
            {
                var domainErrors = new List<Error>();
                await Validate(csvRecord, domainErrors);

                if (domainErrors.Any())
                {
                    bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                        csvRecord.RowNumber,
                        await GetEmployerName(csvRecord.AgreementId),
                        csvRecord.ULN,
                        csvRecord.GivenNames + " " + csvRecord.FamilyName,
                        domainErrors
                        ));
                }
            }

            return new BulkUploadValidateApiResponse
            {
                BulkUploadValidationErrors = bulkUploadValidationErrors
            };
        }

        private async Task Validate(CsvRecord csvRecord, List<Error> domainErrors)
        {
            domainErrors.AddRange(await ValidateAgreementIdValidFormat(csvRecord));
            
            if (!domainErrors.Any())
            {
                domainErrors.AddRange(await ValidateAgreementIdIsSigned(csvRecord));

                // when a valid agreement has not been signed validation will stop
                if(domainErrors.Any())
                    return;

                domainErrors.AddRange(await ValidateAgreementIdMustBeLevy(csvRecord));
            }

            domainErrors.AddRange(await ValidateCohortRef(csvRecord));
            domainErrors.AddRange(ValidateUln(csvRecord));
            domainErrors.AddRange(ValidateFamilyName(csvRecord));
            domainErrors.AddRange(ValidateGivenName(csvRecord));
            domainErrors.AddRange(ValidateDateOfBirth(csvRecord));
            domainErrors.AddRange(ValidateEmailAddress(csvRecord));
            domainErrors.AddRange(ValidateCourseCode(csvRecord));
            domainErrors.AddRange(ValidateStartDate(csvRecord));
            domainErrors.AddRange(ValidateEndDate(csvRecord));
            domainErrors.AddRange(ValidateCost(csvRecord));
            domainErrors.AddRange(ValidateProviderRef(csvRecord));
        }

        private async Task<string> GetEmployerName(string agreementId)
        {
            var employerDetails = await GetEmployerDetails(agreementId);
            return employerDetails.Name;
        }

        private async Task<bool?> IsLevy(string agreementId)
        {
            var employerDetails = await GetEmployerDetails(agreementId);
            return employerDetails.IsLevy;
        }

        private async Task<bool?> IsSigned(string agreementId)
        {
            var employerDetails = await GetEmployerDetails(agreementId);
            return employerDetails.IsSigned;
        }

        private async Task<(string Name, bool? IsLevy, bool? IsSigned)> GetEmployerDetails(string agreementId)
        {
            if (!string.IsNullOrEmpty(agreementId))
            {
                if (_employerNames.ContainsKey(agreementId))
                {
                    var result = _employerNames.GetValueOrDefault(agreementId);
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
                    var tuple = (employerName, isLevy, isSigned);
                    
                    _employerNames.Add(agreementId, tuple);
                    
                    return tuple;
                }
            }

            return (string.Empty, null, null);
        }

        private Models.Cohort GetCohortDetails(string cohortRef)
        {
            var cohort = _dbContext.Value.Cohorts
                .Include(x => x.AccountLegalEntity)
                .Include(x => x.Apprenticeships)
                .Where(x => x.Reference == cohortRef).FirstOrDefault();

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

        private DateTime? GetValidDate(string date, string format)
        {
            DateTime outDateTime;
            if (!string.IsNullOrWhiteSpace(date) && 
                DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out outDateTime))
                return outDateTime;
            return null;
        }
    }
}