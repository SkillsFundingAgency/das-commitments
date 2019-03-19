using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";

        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceName));
                    scan.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
            For<IMediator>().Use<Mediator>();
            For(typeof(IPipelineBehavior<,>)).Use(typeof(ValidationBehavior<,>));
        }
    }
}