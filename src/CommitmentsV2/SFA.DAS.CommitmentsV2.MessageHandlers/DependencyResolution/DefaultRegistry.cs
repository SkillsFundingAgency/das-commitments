using MediatR;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
        }
    }
}