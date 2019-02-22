using MediatR;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public class MeditorRegistry : Registry
    {
        public MeditorRegistry()
        {
            For<IMediator>().Use<Mediator>();
            For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
            For<IRequestHandler<CreateAccountCommand>>().Use<CreateAccountCommandHandler>();
        }
    }
}