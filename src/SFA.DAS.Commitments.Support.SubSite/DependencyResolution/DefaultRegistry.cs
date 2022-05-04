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
    using SFA.DAS.HashingService;
    using SFA.DAS.Learners.Validators;
    using StructureMap;
    using StructureMap.Graph;
    using System.Configuration;
    using System.Web;
    using SFA.DAS.CommitmentsV2.DependencyResolution;

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

                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
            });

            For(typeof(IPipelineBehavior<,>)).Use(typeof(ValidationBehavior<,>));

            //ConfigureLog();

            //For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            //For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));

            For<ISiteValidatorSettings>().Use(ctx => ctx.GetInstance<SiteValidatorSettings>());

            //For<ICommitmentRepository>().Use<CommitmentRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            //For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);

            For<IApprenticeshipsOrchestrator>().Use<ApprenticeshipsOrchestrator>();
            For<IValidator<ApprenticeshipSearchQuery>>().Use<ApprenticeshipsSearchQueryValidator>().Singleton();

            //For<ICurrentDateTime>().Use<CurrentDateTime>();
            //For<IApprenticeshipTransactions>().Use<ApprenticeshipTransactions>();

            For<IApprenticeshipMapper>().Use<ApprenticeshipMapper>();

            For<IHashingService>().Use("Build HashingService", x =>
             {
                 var config = x.GetInstance<CommitmentSupportSiteConfiguartion>();
                 return new HashingService(config.AllowedHashstringCharacters, config.Hashstring);
             });

            // Mediator Handler Mapping
            //For<IAsyncRequestHandler<GetApprenticeshipsByUlnRequest, GetApprenticeshipsByUlnResponse>>().Use<GetApprenticeshipsByUlnQueryHandler>();
            //For<IAsyncRequestHandler<GetCommitmentRequest, GetCommitmentResponse>>().Use<GetCommitmentQueryHandler>();
        }
    }
}