using SFA.DAS.CommitmentsV2.Api.Client.DependencyResolution;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Shared.DependencyInjection
{
    public class CommitmentsSharedRegistry : Registry
    {
        public CommitmentsSharedRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS.Commitments.Shared"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });


            For<ICurrentDateTime>().Use<CurrentDateTime>().Singleton();
            For<ICreateCsvService>().Use<CreateCsvService>().Singleton();

            IncludeRegistry<CommitmentsApiClientRegistry>();
            IncludeRegistry<EncodingRegistry>();
        }
    }
}
