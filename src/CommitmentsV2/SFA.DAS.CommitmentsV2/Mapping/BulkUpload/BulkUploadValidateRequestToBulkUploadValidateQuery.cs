using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadValidateRequestToBulkUploadValidateQuery : IMapper<BulkUploadValidateApiRequest, BulkUploadValidateCommand>
    {
        public Task<BulkUploadValidateCommand> Map(BulkUploadValidateApiRequest source)
        {
            return Task.FromResult(new BulkUploadValidateCommand
            {
                CsvRecords = source.CsvRecords,
                ProviderId = source.ProviderId,
                RplDataExtended = source.RplDataExtended,
                LogId = source.LogId,
                ReservationValidationResults = source.BulkReservationValidationResults,
                ProviderStandardResults = source.ProviderStandardsData
            });
        }
    }
}
