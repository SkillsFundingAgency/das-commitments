using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class BulkUploadValidateApiRequest : SaveDataRequest
    {
        public long ProviderId { get; set; }
        public IEnumerable<CsvRecord> CsvRecords { get; set; }
    }

    public class CsvRecord
    {
        public int RowNumber { get; set; }
        public long CohortId { get; set; }
        public string CohortRef { get; set; }

        public string ULN { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public string DateOfBirth { get; set; }

        public string StdCode { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string TotalPrice { get; set; }

        public string EPAOrgID { get; set; }

        public string ProviderRef { get; set; }

        public string AgreementId { get; set; }
        public long LegalEntityId { get; set; }
        public string EmailAddress { get; set; }
    }
}
