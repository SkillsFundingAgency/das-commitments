using MediatR;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class MediatorRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";

        public MediatorRegistry()
        {
            For<IMediator>().Use<Mediator>().Transient();

            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceName));
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
            });

            For(typeof(IPipelineBehavior<,>)).Use(typeof(ValidationBehavior<,>));
        }
    }
}