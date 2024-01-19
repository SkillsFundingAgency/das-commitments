//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using SFA.DAS.CommitmentsV2.Data;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Azure.Services.AppAuthentication;
//using NServiceBus.Persistence;
//using SFA.DAS.CommitmentsV2.Configuration;
//using SFA.DAS.NServiceBus.SqlServer.Data;
//using SFA.DAS.UnitOfWork.Context;
//using StackExchange.Redis.KeyspaceIsolation;

//namespace SFA.DAS.CommitmentsV2.Startup
//{
//    public static class EntityFrameworkStartup
//    {
//        [ExcludeFromCodeCoverage]
//        public static IServiceCollection AddEntityFramework(this IServiceCollection services, CommitmentsV2Configuration config)
//        {
//            return services.AddScoped(p =>
//            {
//                var unitOfWorkContext = p.GetService<IUnitOfWorkContext>();
//                var azureServiceTokenProvider = new AzureServiceTokenProvider();
//                ProviderCommitmentsDbContext dbContext;
//                try
//                {
//                    var synchronizedStorageSession = unitOfWorkContext.Get<SynchronizedStorageSession>();
//                    var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
//                    var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseSqlServer(sqlStorageSession.Connection);
//                    dbContext = new ProviderCommitmentsDbContext(sqlStorageSession.Connection, config, optionsBuilder.Options, azureServiceTokenProvider);
//                    dbContext.Database.UseTransaction(sqlStorageSession.Transaction);
//                }
//                catch (KeyNotFoundException)
//                {
//                    var connection = DatabaseExtensions.GetSqlConnection(config.DatabaseConnectionString);
//                    var optionsBuilder = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseSqlServer(connection);
//                    dbContext = new ProviderCommitmentsDbContext(optionsBuilder.Options);
//                }

//                return dbContext;
//            });
//        }
//    }
//}
