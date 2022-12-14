using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Services.Shared;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.UnitOfWork.NServiceBus.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IAcademicYearEndExpiryProcessorService>()
                .Use<AcademicYearEndExpiryProcessorService>();

            For<IAcademicYearDateProvider>()
                .Use<AcademicYearDateProvider>().Singleton();

            For<IEventPublisher>()
                .Use<EventPublisher>();

            For<ImportProvidersJobs>();
            For<ImportStandardsJob>();
            For<ImportFrameworksJob>();
            For<AcademicYearEndExpiryProcessorJob>();
            For<IDbContextFactory>().Use<DbContextFactory>();

        }
    }
}