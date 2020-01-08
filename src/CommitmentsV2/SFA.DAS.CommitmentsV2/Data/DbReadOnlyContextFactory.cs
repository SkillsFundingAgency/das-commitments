using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class DbReadOnlyContextFactory : IDbReadOnlyContextFactory
    {
        private readonly string _connectionString;
        private readonly ILoggerFactory _loggerFactory;

        public DbReadOnlyContextFactory(CommitmentsV2Configuration configuration, ILoggerFactory loggerFactory)
        {
            _connectionString = configuration.ReadOnlyDatabaseConnectionString;
            _loggerFactory = loggerFactory;
        }

        public CommitmentsReadOnlyDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<CommitmentsReadOnlyDbContext>()
                .UseSqlServer(_connectionString)
                .UseLoggerFactory(_loggerFactory)
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning));

            var dbContext = new CommitmentsReadOnlyDbContext(optionsBuilder.Options);

            return dbContext;
        }
    }

}