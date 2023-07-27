using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public class BulkUploadValidateCommand : IRequest<BulkUploadValidateApiResponse>
    {
        public long ProviderId { get; set; }
        public IEnumerable<BulkUploadAddDraftApprenticeshipRequest> CsvRecords { get; set; } = new List<BulkUploadAddDraftApprenticeshipRequest>();
        public BulkReservationValidationResults ReservationValidationResults { get; set; } = new BulkReservationValidationResults();
        public ProviderStandardResults ProviderStandardResults { get; set; } = new ProviderStandardResults();
    }
}
