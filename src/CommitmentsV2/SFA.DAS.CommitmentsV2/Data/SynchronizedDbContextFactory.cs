using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NServiceBus.Persistence;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.UnitOfWork;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class SynchronizedDbContextFactory : IDbContextFactory
    {
        private readonly IUnitOfWorkContext _unitOfWorkContext;
        private readonly ILoggerFactory _loggerFactory;

        public SynchronizedDbContextFactory(IUnitOfWorkContext unitOfWorkContext, ILoggerFactory loggerFactory)
        {
            _unitOfWorkContext = unitOfWorkContext;
            _loggerFactory = loggerFactory;
        }

        public ProviderCommitmentsDbContext CreateDbContext()
        {
            var synchronizedStorageSession = _unitOfWorkContext.Find<SynchronizedStorageSession>();
            var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();

            var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseSqlServer(sqlStorageSession.Connection)
                .UseLoggerFactory(_loggerFactory)
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning));

            var dbContext = new ProviderCommitmentsDbContext(optionsBuilder.Options);

            dbContext.Database.UseTransaction(sqlStorageSession.Transaction);

            return dbContext;
        }
    }
}