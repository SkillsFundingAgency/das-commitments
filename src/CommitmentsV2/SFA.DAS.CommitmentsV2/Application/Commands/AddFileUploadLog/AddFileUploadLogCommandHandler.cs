using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;

public class AddFileUploadCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<AddFileUploadLogCommand, BulkUploadAddLogResponse>
{
    public async Task<BulkUploadAddLogResponse> Handle(AddFileUploadLogCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

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

        return new BulkUploadAddLogResponse
        {
            LogId = fileUploadLog.Id
        };
    }
}