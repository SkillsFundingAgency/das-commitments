using FluentValidation;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.Validation;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ValidationRegistry : Registry
    {
        public ValidationRegistry()
        {
            For<IDomainValidator>().Use<DomainValidator>().Singleton();
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(Constants.ServiceName));
                scan.ConnectImplementationsToTypesClosing(typeof(IValidator<>)).OnAddedPluginTypes(expression =>
                {
                    expression.Singleton();
                    System.Console.WriteLine($"Added validator");
                });

                scan.AddAllTypesOf<IValidator>();
            });

            For<IApprenticeshipOverlapService>().Use<ApprenticeshipOverlapService>().Singleton();
        }
    }
}