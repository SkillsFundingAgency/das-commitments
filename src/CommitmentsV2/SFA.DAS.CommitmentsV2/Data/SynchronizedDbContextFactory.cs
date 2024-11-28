using Microsoft.Extensions.Logging;
using NServiceBus.Persistence;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.Data;

public class SynchronizedDbContextFactory(IUnitOfWorkContext unitOfWorkContext, ILoggerFactory loggerFactory)
    : IDbContextFactory
{
    public ProviderCommitmentsDbContext CreateDbContext()
    {
        var synchronizedStorageSession = unitOfWorkContext.Find<SynchronizedStorageSession>();
        var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();

        var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseSqlServer(sqlStorageSession.Connection)
            .UseLoggerFactory(loggerFactory);

        var dbContext = new ProviderCommitmentsDbContext(optionsBuilder.Options);

        dbContext.Database.UseTransaction(sqlStorageSession.Transaction);

        return dbContext;
    }
}