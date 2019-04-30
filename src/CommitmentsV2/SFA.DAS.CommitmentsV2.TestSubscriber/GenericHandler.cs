using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    class GenericHandler 
    {
        public Task Log(object message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received message id:{context.MessageId} type:{message.GetType().FullName} received:{DateTime.Now:T} Content: {JsonConvert.SerializeObject(message)}");
            return Task.CompletedTask;
        }
    }

    class ApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.IApprenticeshipCreatedEvent>
    {
        public Task Handle(IApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DataLockTriageApprovedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.IDataLockTriageApprovedEvent>
    {
        public Task Handle(IDataLockTriageApprovedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipStoppedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.IApprenticeshipStoppedEvent>
    {
        public Task Handle(IApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DraftApprenticeshipDeletedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.IDraftApprenticeshipDeletedEvent>
    {
        public Task Handle(IDraftApprenticeshipDeletedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }
}
