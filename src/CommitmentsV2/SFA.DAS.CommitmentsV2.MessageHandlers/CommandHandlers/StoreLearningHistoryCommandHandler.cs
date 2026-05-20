using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
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
            logger.LogInformation("Handling {TypeName} with MessageId '{MessageId}'", nameof(StoreLearningHistoryCommand), context.MessageId);

            if (!Guid.TryParse(context.MessageId, out var messageId))
            {
                logger.LogError("Invalid MessageId '{MessageId}' for {TypeName}", context.MessageId, nameof(StoreLearningHistoryCommand));
                return;
            }

            var apprenticeship = await dbContext.Value.GetApprenticeshipDetailsAggregate(command.ApprenticeshipId, default);
            var cohort = await dbContext.Value.Cohorts.GetById(apprenticeship.CommitmentId,
                             c => new
                             {
                                 c.EmployerAccountId,
                                 c.AccountLegalEntityId,
                                 c.ProviderId
                             }, default);

            var providerName = await dbContext.Value.Providers.GetById(cohort.ProviderId,
                p => p.Name, default);

            var accountLegalEntityName = await dbContext.Value.AccountLegalEntities.GetById(cohort.AccountLegalEntityId,
                al => al.Name, default);

            var history = dbContext.Value.LearningChangeHistory;
            history.Add(new Models.LearningChangeHistory()
            {
                AccountId = cohort.EmployerAccountId,
                AppliedDate = command.AppliedDate,
                ApprenticeshipId = command.ApprenticeshipId,
                ChangeType = ((byte)command.ChangeType),
                Created = DateTime.UtcNow,
                Description = command.Description,
                LearningKey = command.LearningKey,
                EmployerName = accountLegalEntityName,
                ProviderName = providerName,
                UKPRN = cohort.ProviderId,
                Source = ((byte)command.Source),
                Id = messageId,
                LearnerName = $"{apprenticeship.FirstName} {apprenticeship.LastName}"
            });

            await dbContext.Value.SaveChangesAsync();
            logger.LogInformation("Successfully processed {TypeName} with MessageId '{MessageId}'", nameof(StoreLearningHistoryCommand), context.MessageId);
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogError(dbEx, "Database update error processing {TypeName} with MessageId '{MessageId}'", nameof(StoreLearningHistoryCommand), context.MessageId);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(StoreLearningHistoryCommand));
            throw;
        }
    }
}