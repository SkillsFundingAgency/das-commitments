using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.LearningDataSync
{
    public class LearningDataSyncCommand : IRequest
    {
    }

    public class LearningDataSyncCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<LearningDataSyncCommandHandler> logger) : IRequestHandler<LearningDataSyncCommand>
    {
        public async Task Handle(LearningDataSyncCommand request, CancellationToken cancellationToken)
        {
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

            // Here is the batching logic
            foreach (var batch in ids.Chunk(100))
            {
                // This 'batch' variable now contains up to 100 IDs in each iteration.

                // Placeholder for sending an event/message with this batch of IDs:
                // e.g., await SendBatchEventAsync(batch);

                // For now, just log the size of each batch.
                logger.LogInformation($"Sending batch of {batch.Length} Apprenticeships.");
            }

            logger.LogInformation($"{ids.Count} Apprenticeship records found");
        }
    }
}
