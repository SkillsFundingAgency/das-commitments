using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbConnection _dbConnection;

        public DbContextFactory(DbConnection dbConnection)
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
