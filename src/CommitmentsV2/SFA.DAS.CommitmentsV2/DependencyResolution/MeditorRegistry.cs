using MediatR;
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class MeditorRegistry : Registry
    {
        public MeditorRegistry()
        {
            For<IMediator>().Use<Mediator>();
            For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
            For<IRequestHandler<CreateAccountCommand>>().Use<CreateAccountCommandHandler>();
            For<IRequestHandler<UpdateAccountNameCommand>>().Use<UpdateAccountNameCommandHandler>();
            For<IRequestHandler<AddAccountLegalEntityCommand>>().Use<AddAccountLegalEntityCommandHandler>();
            For<IRequestHandler<RemoveAccountLegalEntityCommand>>().Use<RemoveAccountLegalEntityCommandHandler>();
            For<IRequestHandler<UpdateAccountLegalEntityNameCommand>>().Use<UpdateAccountLegalEntityNameCommandHandler>();
        }
    }
}