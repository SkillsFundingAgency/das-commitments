using MediatR;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ValidatorRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";

        public ValidatorRegistry()
        {
            // The validators are registered automatically by a call to AddFluentValidation in startup

            For(typeof(IPipelineBehavior<,>)).Use(typeof(ValidationBehavior<,>));
        }
    }
}