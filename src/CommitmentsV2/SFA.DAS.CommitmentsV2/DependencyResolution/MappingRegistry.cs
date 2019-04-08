using MediatR;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Mapping;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class MappingRegistry : Registry
    {
        public MappingRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(Constants.ServiceName));
                scan.ConnectImplementationsToTypesClosing(typeof(IMapper<,>));
                scan.ConnectImplementationsToTypesClosing(typeof(IAsyncMapper<,>));
            });
        }
    }
}