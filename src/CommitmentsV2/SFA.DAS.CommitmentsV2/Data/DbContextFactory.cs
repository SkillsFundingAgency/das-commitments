using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class DbContextWithDbConnectionFactory : IDbContextFactory
    {
        private readonly DbConnection _dbConnection;

        public DbContextWithDbConnectionFactory(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public AccountsDbContext CreateAccountsDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(_dbConnection)
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning));

            var dbContext = new AccountsDbContext(optionsBuilder.Options);

            return dbContext;
        }
    }
}
