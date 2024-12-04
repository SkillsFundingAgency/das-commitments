using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Data;

public interface IDbContextFactory
{
    ProviderCommitmentsDbContext CreateDbContext();
}

public class DbContextFactory(DbConnection dbConnection, ILoggerFactory loggerFactory) : IDbContextFactory
{
    public ProviderCommitmentsDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseSqlServer(dbConnection)
            .UseLoggerFactory(loggerFactory);

        return new ProviderCommitmentsDbContext(optionsBuilder.Options);
    }
}