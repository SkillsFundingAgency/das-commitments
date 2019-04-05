using MediatR;
using SFA.DAS.CommitmentsV2.Mapping;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class MappingRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";

        public MappingRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceName));
                scan.ConnectImplementationsToTypesClosing(typeof(IMapper<,>));
                scan.ConnectImplementationsToTypesClosing(typeof(IAsyncMapper<,>));
            });
        }
    }
}