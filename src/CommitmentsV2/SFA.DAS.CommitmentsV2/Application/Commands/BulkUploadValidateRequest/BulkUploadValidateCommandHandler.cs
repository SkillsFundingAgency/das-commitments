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

        //private List<Error> ValidateProviderRef(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (!string.IsNullOrEmpty(csvRecord.ProviderRef) && csvRecord.ProviderRef.Length > 20)
        //    {
        //        domainErrors.Add(new Error("ProviderRef", "The <b>Provider Ref</b> must not be longer than 20 characters"));
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateCost(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.TotalPrice))
        //    {
        //        domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        //    }
        //    else if (!int.TryParse(csvRecord.TotalPrice, out var price))
        //    {
        //        domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        //    }
        //    else if (price == 0)
        //    {
        //        domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be more than £0"));
        //    }
        //    else if (price > 100000)
        //    {
        //        domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be £100,000 or less"));
        //    }
        //    else if (!Regex.IsMatch(csvRecord.TotalPrice, "^([1-9]{1}([0-9]{1,2})?)+(,[0-9]{3})*$|^[1-9]{1}[0-9]*$"))
        //    {
        //        domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateEndDate(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.EndDate))
        //    {
        //        domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
        //    }
        //    else if (!Regex.IsMatch(csvRecord.EndDate, "^\\d\\d\\d\\d-\\d\\d$"))
        //    {
        //        domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
        //    }
        //    else
        //    {
        //        var endDate = GetValidDate(csvRecord.EndDate, "yyyy-MM");
        //        if (endDate == null)
        //        {
        //            domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
        //        }
        //        else
        //        {
        //            var startDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
        //            if (startDate != null && endDate.Value < startDate.Value)
        //            {
        //                domainErrors.Add(new Error("EndDate", "Enter an <b>end date</b> that is after the start date"));
        //            }
        //        }
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateStartDate(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    bool IsBeforeMay2017(DateTime startDate)
        //    {
        //        return startDate < Constants.DasStartDate;
        //    }

        //    bool IsBeforeMay2018AndIsCohortIsTransferFunded(DateTime startDate, string cohortRef)
        //    {
        //        var cohortDetails = GetCohortDetails(cohortRef);
        //        if (cohortDetails != null)
        //        {
        //            return cohortDetails.TransferSenderId.HasValue && startDate < Constants.TransferFeatureStartDate;
        //        }

        //        return false;
        //    }

        //    if (string.IsNullOrEmpty(csvRecord.StartDate))
        //    {
        //        domainErrors.Add(new Error("StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01"));
        //        return domainErrors;
        //    }
        //    else if (!Regex.IsMatch(csvRecord.StartDate, "^\\d\\d\\d\\d-\\d\\d-\\d\\d$"))
        //    {
        //        domainErrors.Add(new Error("StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01"));
        //        return domainErrors;
        //    }
        //    else
        //    {
        //        var startDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
        //        if (startDate != null)
        //        {
        //            if (IsBeforeMay2017(startDate.Value))
        //            {
        //                domainErrors.Add(new Error("StartDate", "The <b>start date</b> must not be earlier than May 2017"));
        //            }
        //            else if (IsBeforeMay2018AndIsCohortIsTransferFunded(startDate.Value, csvRecord.CohortRef))
        //            {
        //                domainErrors.Add(new Error("StartDate", "The <b>start date</b> for apprenticeships funded through a transfer must not be earlier than May 2018"));
        //            }
        //            if (startDate > _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
        //            {
        //                domainErrors.Add(new Error("StartDate", "The <b>start date</b> must be no later than one year after the end of the current teaching year"));
        //            }

        //            var standard = GetStandardDetails(csvRecord.StdCode);
        //            if (standard != null)
        //            {
        //                if (standard.EffectiveFrom.HasValue &&
        //                    startDate < standard.EffectiveFrom.Value)
        //                {
        //                    domainErrors.Add(new Error("StartDate", $"This training course is only available to apprentices with a <b>start date</b> after {standard.EffectiveFrom.Value.Month}  {standard.EffectiveFrom.Value.Year}"));
        //                }

        //                if (standard.EffectiveTo.HasValue &&
        //                  startDate > standard.EffectiveTo.Value)
        //                {
        //                    domainErrors.Add(new Error("StartDate", $"This training course is only available to apprentices with a <b>start date</b> before {standard.EffectiveTo.Value.Month}  {standard.EffectiveTo.Value.Year}"));
        //                }
        //            }
        //        }
        //        else
        //        {
        //            domainErrors.Add(new Error("StartDate", "You must enter the <b>start date</b>, for example 2017-09-01"));
        //        }
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateCourseCode(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.StdCode))
        //    {
        //        domainErrors.Add(new Error("StdCode", "<b>Standard code</b> must be entered"));
        //    }
        //    else if (!csvRecord.StdCode.All(char.IsDigit) && int.TryParse(csvRecord.StdCode, out _))
        //    {
        //        domainErrors.Add(new Error("StdCode", "Enter a valid <b>standard code</b>"));
        //    }
        //    else if (csvRecord.StdCode.Length > 5)
        //    {
        //        domainErrors.Add(new Error("StdCode", "Enter a valid <b>standard code</b>"));
        //    }
        //    else if (GetStandardDetails(csvRecord.StdCode) == null)
        //    {
        //        domainErrors.Add(new Error("StdCode", "Enter a valid <b>standard code</b>"));
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateEmailAddress(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    bool IsAValidEmailAddress(string emailAsString)
        //    {
        //        try
        //        {
        //            var email = new System.Net.Mail.MailAddress(emailAsString);
        //            var parts = email.Address.Split('@');
        //            if (!parts[1].Contains(".") || parts[1].EndsWith("."))
        //            {
        //                return false;
        //            }
        //            return email.Address == emailAsString;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }

        //    EmailOverlapCheckResult OverlapCheckEmail(CsvRecord csvRecord)
        //    {
        //        var learnerStartDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
        //        var learnerEndDate = GetValidDate(csvRecord.EndDate, "yyyy-MM");
        //        if (learnerStartDate.HasValue && learnerEndDate.HasValue)
        //        {
        //            return _overlapService.CheckForEmailOverlaps(csvRecord.EmailAddress, new Domain.Entities.DateRange(learnerStartDate.Value, learnerEndDate.Value), null, null, CancellationToken.None).Result;
        //        }

        //        return new EmailOverlapCheckResult(csvRecord.RowNumber, OverlapStatus.None, false );
        //    }

        //    if (string.IsNullOrEmpty(csvRecord.EmailAddress))
        //    {
        //        domainErrors.Add(new Error("EmailAddress", "<b>Email address</b> must be entered"));
        //    }
        //    else
        //    {
        //        if (!IsAValidEmailAddress(csvRecord.EmailAddress))
        //        {
        //            domainErrors.Add(new Error("EmailAddress", $"Enter a valid <b>email address</b>"));
        //        }
        //        if (csvRecord.EmailAddress.Length > 200)
        //        {
        //            domainErrors.Add(new Error("EmailAddress", "Enter an <b>email address</b> that is not longer than 200 characters"));
        //        }
        //        else
        //        {
        //            var overlapResult = OverlapCheckEmail(csvRecord);
        //            switch (overlapResult.OverlapStatus)
        //            {
        //                case OverlapStatus.DateEmbrace:
        //                case OverlapStatus.DateWithin:
        //                    domainErrors.Add(new Error("EmailAddress", $"The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address"));
        //                    domainErrors.Add(new Error("EmailAddress", $"The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address"));
        //                    break;
        //                case OverlapStatus.OverlappingEndDate:
        //                    domainErrors.Add(new Error("EmailAddress", $"The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address"));
        //                    break;
        //                case OverlapStatus.OverlappingStartDate:
        //                    domainErrors.Add(new Error("EmailAddress", $"The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address"));
        //                    break;
        //            }
        //        }

        //        if (_csvRecords.Any(x => x.EmailAddress == csvRecord.EmailAddress && csvRecord.RowNumber > x.RowNumber))
        //        {
        //            domainErrors.Add(new Error("EmailAddress", $"The <b>email address</b> has already been used for an apprentice in this file"));
        //        }
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateDateOfBirth(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    bool WillApprenticeBeAtLeast15AtStartOfTraining(string startDateTime, DateTime dobDate)
        //    {
        //        var startDate = GetValidDate(startDateTime, "yyyy-MM-dd");
        //        if (startDate == null) return true; // Don't fail validation if both fields not set

        //        int age = startDate.Value.Year - dobDate.Year;
        //        if (startDate < dobDate.AddYears(age)) age--;

        //        return age >= 15;
        //    }

        //    bool ApprenticeAgeMustBeLessThen115AtStartOfTraining(string startDateTime, DateTime dobDate)
        //    {
        //        var startDate = GetValidDate(startDateTime, "yyyy-MM-dd");
        //        if (startDate == null) return true; // Don't fail validation if both fields not set

        //        int age = startDate.Value.Year - dobDate.Year;
        //        if (startDate < dobDate.AddYears(age)) age--;

        //        return age < 115;
        //    }

        //    if (string.IsNullOrEmpty(csvRecord.DateOfBirth))
        //    {
        //        domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));
        //    }
        //    else if (!Regex.IsMatch(csvRecord.DateOfBirth, "^\\d\\d\\d\\d-\\d\\d-\\d\\d$"))
        //    {
        //        domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));

        //    }
        //    else
        //    {
        //        var dateOfBith = GetValidDate(csvRecord.DateOfBirth, "yyyy-MM-dd");
        //        if (dateOfBith == null)
        //        {
        //            domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));
        //        }
        //        else if (!WillApprenticeBeAtLeast15AtStartOfTraining(csvRecord.StartDate, dateOfBith.Value))
        //        {
        //            domainErrors.Add(new Error("DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are at least 15 years old at the start of their training"));
        //        }
        //        else if (!ApprenticeAgeMustBeLessThen115AtStartOfTraining(csvRecord.StartDate, dateOfBith.Value))
        //        {
        //            domainErrors.Add(new Error("DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are not older than 115 years old at the start of their training"));
        //        }
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateFamilyName(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.FamilyName))
        //    {
        //        domainErrors.Add(new Error("FamilyName", "<b>Last name</b> must be entered"));
        //    }
        //    else if (csvRecord.FamilyName.Length > 100)
        //    {
        //        domainErrors.Add(new Error("FamilyName", "Enter a <b>last name</b> that is not longer than 100 characters"));
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateGivenName(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.GivenNames))
        //    {
        //        domainErrors.Add(new Error("GivenName", "<b>Fisrt name</b> must be entered"));
        //        return domainErrors;
        //    }
        //    if (csvRecord.GivenNames.Length > 100)
        //    {
        //        domainErrors.Add(new Error("GivenName", "Enter a <b>first name</b> that is not longer than 100 characters"));
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateUln(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    OverlapCheckResult OverlapCheck(CsvRecord csvRecord)
        //    {
        //        var learnerStartDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
        //        var learnerEndDate = GetValidDate(csvRecord.EndDate, "yyyy-MM");
        //        if (learnerStartDate.HasValue && learnerEndDate.HasValue)
        //        {
        //            return _overlapService.CheckForOverlaps(csvRecord.ULN, new Domain.Entities.DateRange(learnerStartDate.Value, learnerEndDate.Value), null, CancellationToken.None).Result;
        //        }

        //        return new OverlapCheckResult(false, false);
        //    }

        //    if (string.IsNullOrEmpty(csvRecord.ULN))
        //    {
        //        domainErrors.Add(new Error("ULN", "Enter a 10-digit <b>unique learner number</b>"));
        //    }
        //    else
        //    {
        //        if (csvRecord.ULN == "9999999999")
        //        {
        //            domainErrors.Add(new Error("ULN", $"The <b>unique learner number</b> of 9999999999 isn't valid"));
        //        }
        //        else if (csvRecord.ULN.Length != 10)
        //        {
        //            domainErrors.Add(new Error("ULN", "Enter a 10-digit <b>unique learner number</b>"));
        //        }
        //        else if (!Regex.IsMatch(csvRecord.ULN, "^[1-9]{1}[0-9]{9}$"))
        //        {
        //            domainErrors.Add(new Error("ULN", $"Enter a 10-digit <b>unique learner number</b>"));
        //        }
        //        else
        //        {
        //            var overlapResult = OverlapCheck(csvRecord);
        //            if (overlapResult.HasOverlappingStartDate)
        //            {
        //                domainErrors.Add(new Error("ULN", $"The <b>start date</b> overlaps with existing training dates for the same apprentice"));
        //            }
        //            if (overlapResult.HasOverlappingEndDate)
        //            {
        //                domainErrors.Add(new Error("ULN", $"The <b>end date</b> overlaps with existing training dates for the same apprentice"));
        //            }
        //        }

        //        if (_csvRecords.Any(x => x.ULN == csvRecord.ULN && csvRecord.RowNumber > x.RowNumber))
        //        {
        //            domainErrors.Add(new Error("ULN", $"The <b>unique learner number</b> has already been used for an apprentice in this file"));
        //        }
        //    }
        //    return domainErrors;
        //}

        //private List<Error> ValidateCohortRef(CsvRecord csvRecord)
        //{
        //    var domainErrors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.CohortRef))
        //    {
        //        domainErrors.Add(new Error("CohortRef", "<b>Cohort Ref</b> must be entered"));
        //    }
        //    else
        //    {
        //        var cohort = GetCohortDetails(csvRecord.CohortRef);

        //        if (cohort == null)
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"You must enter a valid <br>Cohort Ref</b>"));
        //            return domainErrors;
        //        }
        //        else if (csvRecord.CohortRef.Length > 20)
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"You must enter a valid <br>Cohort Ref</b>"));
        //        }
        //        else if (cohort.AccountLegalEntity.PublicHashedId != csvRecord.AgreementId && !string.IsNullOrWhiteSpace(GetEmployerName(csvRecord.AgreementId)))
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"You must enter a valid <b>Cohort Ref</b>"));
        //        }

        //        if (cohort.WithParty == Types.Party.Employer)
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort, as it is with the employer. You need to <b>add this learner to a different or new cohort.</b>"));
        //        }
        //        if (cohort.WithParty == Types.Party.TransferSender)
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort, as it is with the transfer sending employer. You need to <b>add this learner to a different or new cohort.</b>"));
        //        }
        //        if (cohort.IsLinkedToChangeOfPartyRequest)
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort. You need to <b>add this learner to a different or new cohort.</b>"));
        //        }
        //        if (cohort.Apprenticeships.Count > 0)
        //        {
        //            domainErrors.Add(new Error("CohortRef", $"This cohort is not empty. You need to <b>add this learner to a different or new cohort.</b>"));
        //        }
        //    }

        //    return domainErrors;
        //}

        //private List<Error> ValidateAgreementId(CsvRecord csvRecord)
        //{
        //    List<Error> errors = new List<Error>();
        //    if (string.IsNullOrEmpty(csvRecord.AgreementId))
        //    {
        //        errors.Add(new Error("AgreementId", "<b>Agreement ID</b> must be entered"));
        //    }
        //    else if (!csvRecord.AgreementId.All(char.IsLetterOrDigit))
        //    {
        //        errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        //    }
        //    else if (csvRecord.AgreementId.Length > 6)
        //    {
        //        errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        //    }
        //    else if (string.IsNullOrWhiteSpace(GetEmployerName(csvRecord.AgreementId)))
        //    {
        //        errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        //    }
        //    else if (!IsLevy(csvRecord.AgreementId).Value)
        //    {
        //        errors.Add(new Error("AgreementId", $"You cannot add apprentices via file on behalf of <b>non-levy employers</b> yet. "));
        //    }

        //    return errors;
        //}

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