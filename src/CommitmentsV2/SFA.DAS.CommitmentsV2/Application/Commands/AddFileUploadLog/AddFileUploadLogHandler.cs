using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog
{
    public class AddFileUploadHandler : IRequestHandler<AddFileUploadLogCommand, AddFileUploadLogResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public AddFileUploadHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AddFileUploadLogResult> Handle(AddFileUploadLogCommand command, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var fileUploadLog = new FileUploadLog
            {
                ProviderId = command.ProviderId,
                FileName = command.FileName,
                RplCount = command.RplCount,
                RowCount = command.RowCount,
                FileContent = command.FileContent
            };


            db.FileUploadLogs.Add(fileUploadLog);
            await db.SaveChangesAsync(cancellationToken);

            var response = new AddFileUploadLogResult
            {
                LogId = fileUploadLog.Id
            };

            return response;
        }
    }
}
