using SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob;
using SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Infrastructure.Data;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Services.Shared;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.UnitOfWork.DependencyResolution.StructureMap;
using StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.Services;
using SFA.DAS.UnitOfWork.Pipeline;


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

            For<IAcademicYearEndExpiryProcessor>()
                .Use<AcademicYearEndExpiryProcessor>();

            For<IAcademicYearDateProvider>()
                .Use<AcademicYearDateProvider>();

            For<IEventPublisher>()
                .Use<EventPublisher>();

            For<ImportProvidersJobs>();
            For<ImportStandardsJob>();
            For<ImportFrameworksJob>();
            For<Job>();
            For<IDbContextFactory>().Use<DbContextFactory>();

        }
    }
}