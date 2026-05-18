using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
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

            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, default);

            var history = dbContext.Value.LearningChangeHistory;
            history.Add(new Models.LearningChangeHistory()
            {
                AccountId = apprenticeship.Cohort.EmployerAccountId,
                AppliedDate = command.AppliedDate,
                ApprenticeshipId = command.ApprenticeshipId,
                ChangeType = ((byte)command.ChangeType),
                Created = DateTime.UtcNow,
                Description = command.Description,
                LearnerKey = command.LearningKey,
                EmployerName = apprenticeship.Cohort.AccountLegalEntity.Name,
                ProviderName = apprenticeship.Cohort.Provider.Name,
                UKPRN = apprenticeship.Cohort.ProviderId,
                Source = ((byte)command.Source),
                Id = Guid.NewGuid(),
                LearnerName = $"{apprenticeship.FirstName} {apprenticeship.LastName}"
            });

            await dbContext.Value.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing {TypeName}", nameof(StoreLearningHistoryCommand));
            throw;
        }
    }
}