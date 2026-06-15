using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class StoreLearningHistoryCommandHandler(ILogger<StoreLearningHistoryCommandHandler> logger,
    Lazy<ProviderCommitmentsDbContext> dbContext)
    : IHandleMessages<StoreLearningHistoryCommand>
{
    public async Task Handle(StoreLearningHistoryCommand command, IMessageHandlerContext context)
    {
        try
        {
            if (command is null)
            {
                logger.LogError("Received null command for {TypeName}", nameof(StoreLearningHistoryCommand));
                throw new ArgumentNullException(nameof(command));
            }

            logger.LogInformation("Handling {TypeName} with MessageId '{MessageId}'", nameof(StoreLearningHistoryCommand), context.MessageId);

            if (!Guid.TryParse(context.MessageId, out var messageId))
            {
                logger.LogError("Invalid MessageId '{MessageId}' for {TypeName}", context.MessageId, nameof(StoreLearningHistoryCommand));
                return;
            }

            var isAlreadyProcessed = await dbContext.Value.LearningChangeHistory.AnyAsync(h => h.Id == messageId, default);
            if (isAlreadyProcessed)
            {
                logger.LogInformation("MessageId '{MessageId}' for {TypeName} has already been processed", context.MessageId, nameof(StoreLearningHistoryCommand));
                return;
            }

            var apprenticeship = await dbContext.Value.Apprenticeships
                 .AsNoTracking()
                 .Where(a => a.Id == command.ApprenticeshipId)
                 .Select(a => new
                 {
                     a.FirstName,
                     a.LastName,
                     AccountId = a.Cohort.EmployerAccountId,
                     ProviderName = a.Cohort.Provider.Name,
                     UKPRN = a.Cohort.ProviderId,
                     EmployerName = a.Cohort.AccountLegalEntity.Name,
                 })
                 .SingleOrDefaultAsync(default)
                 ?? throw new BadRequestException($"Apprenticeship {command.ApprenticeshipId} was not found");

            var history = dbContext.Value.LearningChangeHistory;
            history.Add(new Models.LearningChangeHistory()
            {
                AccountId = apprenticeship.AccountId,
                AppliedDate = command.AppliedDate,
                ApprenticeshipId = command.ApprenticeshipId,
                ChangeType = ((byte)command.ChangeType),
                Created = DateTime.UtcNow,
                Description = command.Description,
                LearningKey = command.LearningKey,
                EmployerName = apprenticeship.EmployerName,
                ProviderName = apprenticeship.ProviderName,
                UKPRN = apprenticeship.UKPRN,
                Source = ((byte)command.Source),
                Id = messageId,
                LearnerName = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
                UserId = command.UserId
            });

            await dbContext.Value.SaveChangesAsync();
            logger.LogInformation("Successfully processed {TypeName} with MessageId '{MessageId}'", nameof(StoreLearningHistoryCommand), context.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(StoreLearningHistoryCommand));
            throw;
        }
    }
}