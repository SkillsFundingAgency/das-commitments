using System;
using System.Collections.Generic;
using System.Globalization;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class BulkUploadAddDraftApprenticeshipsRequest : SaveDataRequest
    {
        public long ProviderId { get; set; }
        public long? LogId { get; set; }
        public IEnumerable<BulkUploadAddDraftApprenticeshipRequest> BulkUploadDraftApprenticeships { get; set; }
    }

    public class BulkUploadAddDraftApprenticeshipRequest
    {
        public long? LegalEntityId { get; set; }
        public string AgreementId { get; set; }
        public long? CohortId { get; set; }
        public string CohortRef { get; set; }
        public string UserId { get; set; }
        public long ProviderId { get; set; }
        public string CourseCode { get; set; }
        public DateTime? StartDate => GetDate(StartDateAsString, "yyyy-MM-dd", true);
        public string StartDateAsString { get; set; }
        public DateTime? EndDate => GetDate(EndDateAsString, "yyyy-MM");
        public string EndDateAsString { get; set; }
        public string OriginatorReference { get; set; }
        public Guid? ReservationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth => GetDate(DateOfBirthAsString, "yyyy-MM-dd");
        public string DateOfBirthAsString { get; set; }
        public string Uln { get; set; }
        public string ProviderRef { get; set; }
        public int RowNumber { get; set; }
        public string CostAsString { get; set; }
        public int? Cost
        {
            get
            {
                if (int.TryParse(CostAsString, out var result))
                    return result;
                return null;
            }
        }

        public static DateTime? GetDate(string date, string format, bool isStartDate = false)
        {
            if (!string.IsNullOrWhiteSpace(date) &&
                DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outDateTime))
            {
                if (isStartDate)
                {
                    return new DateTime(outDateTime.Year, outDateTime.Month, 1);
                }

                return outDateTime;
            }
            return null;
        }
        public string EPAOrgId { get; set; }

        public string RecognisePriorLearningAsString { get; set; }
        public bool? RecognisePriorLearning
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RecognisePriorLearningAsString))
                    return null;

                return RecognisePriorLearningAsString?.ToLower() switch
                {
                    "true" => true,
                    "1" => true,
                    "yes" => true,
                    "false" => false,
                    "0" => false,
                    "no" => false,
                    _ => null,
                };
            }
        }

        public string DurationReducedByAsString { get; set; }
        public int? DurationReducedBy
        {
            get
            {
                if(int.TryParse(DurationReducedByAsString, out var result))
                    return result;
                return  null;
            }
        }

        public string PriceReducedByAsString { get; set; }
        public int? PriceReducedBy
        {
            get
            {
                if(int.TryParse(PriceReducedByAsString, out var result))
                    return result;
                return null;
            }
        }
        
        public string TrainingTotalHoursAsString { get; set; }
        public int? TrainingTotalHours
        {
            get
            {
                if (int.TryParse(TrainingTotalHoursAsString, out var result))
                    return result;
                return null;
            }
        }

        public string TrainingHoursReductionAsString { get; set; }
        public int? TrainingHoursReduction
        {
            get
            {
                if (int.TryParse(TrainingHoursReductionAsString, out var result))
                    return result;
                return null;
            }
        }

        public string IsDurationReducedByRPLAsString { get; set; }
        public bool? IsDurationReducedByRPL
        {
            get
            {
                if (string.IsNullOrWhiteSpace(IsDurationReducedByRPLAsString))
                    return null;

                return IsDurationReducedByRPLAsString?.ToLower() switch
                {
                    "true" => true,
                    "1" => true,
                    "yes" => true,
                    "false" => false,
                    "0" => false,
                    "no" => false,
                    _ => null,
                };
            }
        }
    }
}
