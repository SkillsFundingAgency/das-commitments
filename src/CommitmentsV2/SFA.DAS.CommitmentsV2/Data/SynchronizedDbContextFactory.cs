using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus.Persistence;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.Data
{
    // TODO Feels like this should no longer be needed
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
                .UseLoggerFactory(_loggerFactory);

            var dbContext = new ProviderCommitmentsDbContext(optionsBuilder.Options);

            dbContext.Database.UseTransaction(sqlStorageSession.Transaction);

            return dbContext;
        }
    }
}