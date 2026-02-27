using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Application.Commands.LearningDataSync
{
    public class LearningDataSyncCommand : IRequest
    {
    }

    public class LearningDataSyncCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IMessageSession messageSession, ILogger<LearningDataSyncCommandHandler> logger) : IRequestHandler<LearningDataSyncCommand>
    {
        public async Task Handle(LearningDataSyncCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var academicYearStart = new DateTime(2025, 8, 1);
            var academicYearEnd = new DateTime(2026, 7, 31);

            var ids = await dbContext.Value.Apprenticeships
                .Where(x =>
                    x.IsApproved == true &&
                    x.StartDate <= academicYearEnd &&
                    ((x.StopDate ?? x.EndDate) == null || (x.StopDate ?? x.EndDate) >= academicYearStart))
                .Select(x => x.Id)
                .ToListAsync();

            logger.LogInformation($"{ids.Count} Apprenticeship records found");

            var i = 0;
            foreach (var batch in ids.Chunk(20))
            {
                i++;
                var syncBatchCommand = new SyncLearningDataBatchCommand
                {
                    BatchNumber = i,
                    Ids = batch
                };

                logger.LogInformation($"Sending batch of {batch.Length} Apprenticeships.");

                await messageSession.Send(syncBatchCommand);
            }

            stopwatch.Stop();
            logger.LogInformation($"LearningDataSyncCommandHandler completed in {stopwatch.ElapsedMilliseconds}");
        }
    }
}
