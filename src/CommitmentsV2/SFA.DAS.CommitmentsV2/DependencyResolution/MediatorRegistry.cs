using MediatR;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class MediatorRegistry : Registry
    {
        public MediatorRegistry()
        {
            For<IMediator>().Use<Mediator>();
            For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);

            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(Constants.ServiceName));
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
            });

            For(typeof(IPipelineBehavior<,>)).Use(typeof(ValidationBehavior<,>));
        }
    }
}