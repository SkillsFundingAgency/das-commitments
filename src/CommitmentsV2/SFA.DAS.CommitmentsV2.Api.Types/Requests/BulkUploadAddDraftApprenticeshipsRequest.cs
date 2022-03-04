using System;
using System.Collections.Generic;
using System.Globalization;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class BulkUploadAddDraftApprenticeshipsRequest : SaveDataRequest
    {
        public long ProviderId { get; set; }
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
        public DateTime? StartDate => GetDate(StartDateAsString, "yyyy-MM-dd");
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
                if (int.TryParse(CostAsString, out var price))
                {
                    return price;
                }

                return null;
            }
        }

        public static DateTime? GetDate(string date, string format)
        {
            if (!string.IsNullOrWhiteSpace(date) &&
                DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outDateTime))
                return outDateTime;
            return null;
        }
    }
}
