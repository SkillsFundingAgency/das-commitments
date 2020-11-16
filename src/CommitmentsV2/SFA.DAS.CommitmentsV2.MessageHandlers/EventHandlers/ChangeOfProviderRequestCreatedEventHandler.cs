using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
using SFA.DAS.ProviderUrlHelper;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ChangeOfProviderRequestCreatedEventHandler : IHandleMessages<ChangeOfProviderRequestCreatedEvent>
    {
        private readonly IMediator _mediator;
        //private readonly ILinkGenerator _linkGenerator;

        public ChangeOfProviderRequestCreatedEventHandler(IMediator mediator) //, ILinkGenerator linkGenerator)
        {
            _mediator = mediator;
           // _linkGenerator = linkGenerator;
        }
        public async Task Handle(ChangeOfProviderRequestCreatedEvent message, IMessageHandlerContext context)
        {
            var provider = await _mediator.Send(new GetProviderQuery(message.ProviderId));

            var requestUrl = "www.google.com"; //_linkGenerator.ProviderApprenticeshipServiceLink($"{message.ProviderId}/apprentices/{message.CohortReference}/details");
            
            var tokens = new Dictionary<string, string>
            {
                { "TrainingProviderName" , provider.Name },
                { "EmployerName" , message.EmployerName },
                { "ApprenticeNamePossessive" , message.ApprenticeName.EndsWith("s") ? message.ApprenticeName + "'" : message.ApprenticeName + "'s" },
                { "RequestUrl", requestUrl }
            };

            await context.Send(new SendEmailToProviderCommand(message.ProviderId, "ProviderApprenticeshipChangeOfProviderRequested_dev", tokens));
        }
    }
}
