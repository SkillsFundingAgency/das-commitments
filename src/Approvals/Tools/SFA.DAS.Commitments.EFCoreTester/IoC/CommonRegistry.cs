using SFA.DAS.Commitments.EFCoreTester.Config;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;
using SFA.DAS.Commitments.EFCoreTester.Timing;
using StructureMap;

namespace SFA.DAS.Commitments.EFCoreTester.IoC
{
    class CommonRegistry : Registry
    {
        public CommonRegistry(string configLocation)
        {
            For<IConfigProvider>().Use(new ConfigProvider(configLocation)).Singleton();
            For<ITimer>().Use<Timer>().Singleton();

            Scan(scan =>
            {
                scan.TheCallingAssembly();
                //scan.AssembliesFromApplicationBaseDirectory(assembly => assembly.FullName.Contains("SFA.DAS.EAS.LevyAnalyzer"));
                scan.AddAllTypesOf<ICommand>();

                scan.RegisterConcreteTypesAgainstTheFirstInterface();
            });
        }
    }
}
