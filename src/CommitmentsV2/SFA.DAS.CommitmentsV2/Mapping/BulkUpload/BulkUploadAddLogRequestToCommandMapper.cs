using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadAddLogRequestToCommandMapper : IMapper<AddFileUploadLogRequest, AddFileUploadLogCommand>
    {
        public Task<AddFileUploadLogCommand> Map(AddFileUploadLogRequest source)
        {
            return Task.FromResult(new AddFileUploadLogCommand
            {
                ProviderId = source.ProviderId,
                FileContent = source.FileContent,
                FileName = source.FileName,
                RowCount = source.RowCount,
                RplCount = source.RplCount
            });
        }
    }
}
