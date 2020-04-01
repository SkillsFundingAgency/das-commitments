using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class LevyAddedToAccountEventHandler : IHandleMessages<LevyAddedToAccount>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LevyAddedToAccountEventHandler> _logger;

        public LevyAddedToAccountEventHandler(IMediator mediator, ILogger<LevyAddedToAccountEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(LevyAddedToAccount message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"LevyAddedToAccount event received for Account {message.AccountId}");
            throw new NotImplementedException();
        }
    }
}
