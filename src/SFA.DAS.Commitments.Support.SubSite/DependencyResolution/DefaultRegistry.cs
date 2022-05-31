namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution
{
    using FluentValidation;
    using MediatR;
    using Microsoft.Azure;
    using SFA.DAS.Commitments.Support.SubSite.Configuration;
    using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
    using SFA.DAS.Commitments.Support.SubSite.Mappers;
    using SFA.DAS.Commitments.Support.SubSite.Models;
    using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
    using SFA.DAS.Commitments.Support.SubSite.Validation;
    using SFA.DAS.Configuration;
    using SFA.DAS.Configuration.AzureTableStorage;
    using SFA.DAS.Learners.Validators;
    using StructureMap;
    using StructureMap.Graph;
    using System.Configuration;
    using System.Web;
    using SFA.DAS.CommitmentsV2.DependencyResolution;
    using SFA.DAS.Authorization.Services;

    public class DefaultRegistry : Registry
    {
        private const string Version = "1.0";

        public DefaultRegistry()
        {
            For<IMediator>().Use<Mediator>();
            For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);

            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();

                scan.AssembliesFromApplicationBaseDirectory(a =>
                a.GetName().Name.StartsWith("SFA.DAS.Commitments.Support.SubSite") ||
                a.GetName().Name.StartsWith("SFA.DAS.CommitmentsV2"));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
            });

            For(typeof(IPipelineBehavior<,>)).Use(typeof(ValidationBehavior<,>));

            For<IApprenticeshipMapper>().Use<ApprenticeshipMapper>();

            For<IApprenticeshipsOrchestrator>().Use<ApprenticeshipsOrchestrator>();
            For<IValidator<ApprenticeshipSearchQuery>>().Use<ApprenticeshipsSearchQueryValidator>().Singleton();
            For<ISiteValidatorSettings>().Use(ctx => ctx.GetInstance<SiteValidatorSettings>());
        }
    }
}