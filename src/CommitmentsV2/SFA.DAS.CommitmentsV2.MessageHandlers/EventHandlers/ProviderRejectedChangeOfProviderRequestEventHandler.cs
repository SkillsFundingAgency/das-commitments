using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ProviderRejectedChangeOfProviderRequestEventHandler : IHandleMessages<ProviderRejectedChangeOfProviderRequestEvent>
    {
        private readonly IEncodingService _encodingService;
        private readonly IMediator _mediator;

        public ProviderRejectedChangeOfProviderRequestEventHandler(IEncodingService encodingService, IMediator mediator)
        {
            _encodingService = encodingService;
            _mediator = mediator;
        }

        public async Task Handle(ProviderRejectedChangeOfProviderRequestEvent message, IMessageHandlerContext context)
        {
            var changeOfPartyRequest = await _mediator.Send(new GetChangeOfPartyRequestQuery(message.ChangeOfPartyRequestId));

            var sendEmailCommand = new SendEmailToEmployerCommand(message.EmployerAccountId, 
                "TrainingProviderRejectedChangeOfProviderCohort",
                new Dictionary<string, string>
                {
                    { "EmployerName", message.EmployerName },
                    { "TrainingProviderName", message.TrainingProviderName },
                    { "ApprenticeNamePossessive", message.ApprenticeName.EndsWith("s") ? message.ApprenticeName + "'" : message.ApprenticeName + "'s" },
                    { "AccountHashedId", _encodingService.Encode(message.EmployerAccountId, EncodingType.AccountId) },
                    { "ApprenticeshipHashedId", _encodingService.Encode(changeOfPartyRequest.ApprenticeshipId, EncodingType.ApprenticeshipId) }
                }
                );

            await context.Send(sendEmailCommand, new SendOptions());
        }
    }
}
