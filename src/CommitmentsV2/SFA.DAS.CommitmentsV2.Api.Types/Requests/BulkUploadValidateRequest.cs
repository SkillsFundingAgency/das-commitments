using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class BulkUploadValidateApiRequest : SaveDataRequest
{
    public long ProviderId { get; set; }
    public bool RplDataExtended { get; set; }
    public IEnumerable<BulkUploadAddDraftApprenticeshipRequest> CsvRecords { get; set; }
    public BulkReservationValidationResults BulkReservationValidationResults { get; set; }
    public ProviderStandardResults ProviderStandardsData { get; set; }
}