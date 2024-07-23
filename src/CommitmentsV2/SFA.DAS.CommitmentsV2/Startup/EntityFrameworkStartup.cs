using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Persistence;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.Startup;

public static class EntityFrameworkStartup
{
    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, CommitmentsV2Configuration config)
    {
        return services.AddScoped(p =>
        {
            var unitOfWorkContext = p.GetService<IUnitOfWorkContext>();
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            ProviderCommitmentsDbContext dbContext;
                
            try
            {
                var synchronizedStorageSession = unitOfWorkContext.Get<SynchronizedStorageSession>();
                var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
                var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseSqlServer(sqlStorageSession.Connection);
                dbContext = new ProviderCommitmentsDbContext(sqlStorageSession.Connection, config, azureServiceTokenProvider, optionsBuilder.Options);
                dbContext.Database.UseTransaction(sqlStorageSession.Transaction);
            }
            catch (KeyNotFoundException)
            {
                var connection = DatabaseExtensions.GetSqlConnection(config.DatabaseConnectionString);
                var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseSqlServer(connection);
                dbContext = new ProviderCommitmentsDbContext(optionsBuilder.Options);
            }

            return dbContext;
        });
    }
}