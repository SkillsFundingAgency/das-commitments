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
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.ProviderRelationships.Api.Client;

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
        private List<BulkUploadAddDraftApprenticeshipRequest> _csvRecords;

        public long ProviderId { get; set; }

        public BulkUploadValidateCommandHandler(
            ILogger<BulkUploadValidateCommandHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IOverlapCheckService overlapService,
            IAcademicYearDateProvider academicYearDateProvider,
            IProviderRelationshipsApiClient providerRelationshipsApiClient)
        {
            _logger = logger;
            _dbContext = dbContext;
            _employerSummaries = new EmployerSummaries();
            _overlapService = overlapService;
            _academicYearDateProvider = academicYearDateProvider;
            _providerRelationshipsApiClient = providerRelationshipsApiClient;
        }

        public Task<BulkUploadValidateApiResponse> Handle(BulkUploadValidateCommand command, CancellationToken cancellationToken)
        {
            ProviderId = command.ProviderId;
            var bulkUploadValidationErrors = new List<BulkUploadValidationError>();
            _csvRecords = command.CsvRecords.ToList();
            foreach (var csvRecord in command.CsvRecords)
            {
                var domainErrors = new List<Error>();
                domainErrors.AddRange(ValidateAgreementId(csvRecord));
                domainErrors.AddRange(ValidateCohortRef(csvRecord, command.ProviderId));
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
                domainErrors.AddRange(ValidateEPAOrgId(csvRecord));
                if (domainErrors.Count > 0)
                {
                    bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                        csvRecord.RowNumber,
                        GetEmployerName(csvRecord.AgreementId),
                        csvRecord.Uln,
                        csvRecord.FirstName + " " + csvRecord.LastName,
                        domainErrors
                        ));
                }
            }

            return Task.FromResult(new BulkUploadValidateApiResponse
            {
                BulkUploadValidationErrors = bulkUploadValidationErrors
            });
        }

        private string GetEmployerName(string agreementId)
        {
            var employerDetails = GetEmployerDetails(agreementId);
            return employerDetails.Name;
        }

        private bool? IsLevy(string agreementId)
        {
            var employerDetails = GetEmployerDetails(agreementId);
            return employerDetails.IsLevy;
        }

        private EmployerSummary GetEmployerDetails(string agreementId)
        {
            if (!string.IsNullOrEmpty(agreementId))
            {
                if (_employerSummaries.ContainsKey(agreementId))
                {
                    var result = _employerSummaries.GetValueOrDefault(agreementId);
                    return result;
                }
                var accontLegalEntity = _dbContext.Value.AccountLegalEntities
                  .Include(x => x.Account)
                  .Where(x => x.PublicHashedId == agreementId).FirstOrDefault();
                if (accontLegalEntity != null)
                {
                    var employerName = accontLegalEntity.Account.Name;
                    var isLevy = accontLegalEntity.Account.LevyStatus == Types.ApprenticeshipEmployerType.Levy;
                    var employerSummary = new EmployerSummary(agreementId, accontLegalEntity.Id, isLevy, employerName);
                    _employerSummaries.Add(employerSummary);
                    return employerSummary;
                }
            }

            return new EmployerSummary(agreementId, null, null, string.Empty);
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

        private class EmployerSummaries : List<EmployerSummary>
        {
            internal bool ContainsKey(string agreementId)
            {
                return this.Any(x => x.AgreementId == agreementId);
            }

            internal EmployerSummary GetValueOrDefault(string agreementId)
            {
               return this.First(x => x.AgreementId == agreementId);
            }
        }


        private class EmployerSummary
        {
            public EmployerSummary(string agreementId, long? legalEntityId, bool? isLevy, string name)
            {
                AgreementId = agreementId;
                LegalEntityId = legalEntityId;
                IsLevy = isLevy;
                Name = name;
            }

            public string AgreementId { get; set; }
            public long? LegalEntityId { get; set; }
            public bool? IsLevy { get; set; }
            public string Name { get; set; }
        }
    }
}