using SFA.DAS.CommitmentsV2.Api.Client.DependencyResolution;
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

            IncludeRegistry<CommitmentsSharedConfigurationRegistry>();
            IncludeRegistry<ApprenticeshipInfoServiceRegistry>();
            IncludeRegistry<CommitmentsApiClientRegistry>();
            IncludeRegistry<EncodingRegistry>();
        }
    }
}
