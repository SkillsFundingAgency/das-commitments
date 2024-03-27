using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog
{
    public class AddFileUploadCommandHandler : IRequestHandler<AddFileUploadLogCommand, BulkUploadAddLogResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public AddFileUploadCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BulkUploadAddLogResponse> Handle(AddFileUploadLogCommand command, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var fileUploadLog = new FileUploadLog
            {
                ProviderId = command.ProviderId,
                FileName = command.FileName,
                RplCount = command.RplCount,
                RowCount = command.RowCount,
                FileContent = command.FileContent,
                CreatedOn = DateTime.UtcNow
            };

            db.FileUploadLogs.Add(fileUploadLog);
            await db.SaveChangesAsync(cancellationToken);

            var response = new BulkUploadAddLogResponse
            {
                LogId = fileUploadLog.Id
            };

            return response;
        }
    }
}
