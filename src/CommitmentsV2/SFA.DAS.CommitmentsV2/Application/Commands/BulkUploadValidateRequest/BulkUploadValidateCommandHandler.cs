﻿using System;
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
        private readonly Dictionary<string, (string Name, bool? IsLevy)> _employerNames;
        private readonly IOverlapCheckService _overlapService;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private List<CsvRecord> _csvRecords;

        public long ProviderId { get; set; }

        public BulkUploadValidateCommandHandler(
            ILogger<BulkUploadValidateCommandHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IOverlapCheckService overlapService,
            IAcademicYearDateProvider academicYearDateProvider)
        {
            _logger = logger;
            _dbContext = dbContext;
            _employerNames = new Dictionary<string, (string Name, bool? IsLevy)>();
            _overlapService = overlapService;
            _academicYearDateProvider = academicYearDateProvider;
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
                domainErrors.AddRange(ValidateCohortRef(csvRecord));
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
                if (domainErrors.Count > 0)
                {
                    bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                        csvRecord.RowNumber,
                        GetEmployerName(csvRecord.AgreementId),
                        csvRecord.ULN,
                        csvRecord.GivenNames + " " + csvRecord.FamilyName,
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

        private (string Name, bool? IsLevy) GetEmployerDetails(string agreementId)
        {
            if (!string.IsNullOrEmpty(agreementId))
            {
                if (_employerNames.ContainsKey(agreementId))
                {
                    var result = _employerNames.GetValueOrDefault(agreementId);
                    return result;
                }
                var accontLegalEntity = _dbContext.Value.AccountLegalEntities
                  .Include(x => x.Account)
                  .Where(x => x.PublicHashedId == agreementId).FirstOrDefault();
                if (accontLegalEntity != null)
                {
                    var employerName = accontLegalEntity.Account.Name;
                    var isLevy = accontLegalEntity.Account.LevyStatus == Types.ApprenticeshipEmployerType.Levy;
                    var tuple = (employerName, isLevy);
                    _employerNames.Add(agreementId, tuple);
                    return tuple;
                }
            }

            return (string.Empty, null);
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