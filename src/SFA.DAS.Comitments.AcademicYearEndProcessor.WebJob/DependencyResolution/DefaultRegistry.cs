using SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.NLog.Logger;
using StructureMap;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesAndExecutablesFromApplicationBaseDirectory( a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });
            For<ILog>().Use(x => new NLogLogger(x.ParentType, new DummyRequestContext(), null)).AlwaysUnique();
        }
    }
    public class DummyRequestContext : IRequestContext
    {
        public string Url { get; }

        public string IpAddress { get; }
    }
}
