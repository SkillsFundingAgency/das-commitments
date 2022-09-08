using SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob;
using SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Infrastructure.Data;
using SFA.DAS.CommitmentsV2.Infrastructure.Data.Transactions;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {

            For<IDataLockRepository>()
                .Use<DataLockRepository>()
                .Ctor<string>("connectionString").Is(ctx => ctx.GetInstance<CommitmentsV2Configuration>().DatabaseConnectionString);

            For<IApprenticeshipUpdateRepository>()
                .Use<ApprenticeshipUpdateRepository>()
                .Ctor<string>("connectionString").Is(ctx => ctx.GetInstance<CommitmentsV2Configuration>().DatabaseConnectionString);

            For<IApprenticeshipRepository>()
                .Use<ApprenticeshipRepository>()
                .Ctor<string>("connectionString").Is(ctx => ctx.GetInstance<CommitmentsV2Configuration>().DatabaseConnectionString);

            For<IApprenticeshipTransactions>().Use<ApprenticeshipTransactions>();
            For<IApprenticeshipUpdateTransactions>().Use<ApprenticeshipUpdateTransactions>();
            For<IDataLockTransactions>().Use<DataLockTransactions>();
            For<ImportProvidersJobs>();
            For<ImportStandardsJob>();
            For<ImportFrameworksJob>();
            For<AcademicYearEndExpiryProcessor>();
            For<Job>();
            For<IDbContextFactory>().Use<DbContextFactory>();

        }
    }
}