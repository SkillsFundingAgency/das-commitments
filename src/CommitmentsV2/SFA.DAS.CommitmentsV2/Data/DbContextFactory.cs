using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbConnection _dbConnection;
        private readonly ILoggerFactory _loggerFactory;

        public DbContextFactory(DbConnection dbConnection, ILoggerFactory loggerFactory)
        {
            _dbConnection = dbConnection;
            _loggerFactory = loggerFactory;
        }

        public CommitmentsDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<CommitmentsDbContext>()
                .UseSqlServer(_dbConnection)
                .UseLoggerFactory(_loggerFactory)
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning));

            var dbContext = new CommitmentsDbContext(optionsBuilder.Options);

            return dbContext;
        }
    }
}