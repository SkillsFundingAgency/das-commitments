using System;
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

    class ApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipCreatedEvent>
    {
        public Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipPausedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipPausedEvent>
    {
        public Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipResumedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipResumedEvent>
    {
        public Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipStoppedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipStoppedEvent>
    {
        public Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipStopDateChangedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipStopDateChangedEvent>
    {
        public Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipUpdatedApprovedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DataLockTriageApprovedEventHandler : GenericHandler, IHandleMessages<DataLockTriageApprovedEvent>
    {
        public Task Handle(DataLockTriageApprovedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DraftApprenticeshipDeletedEventHandler : GenericHandler, IHandleMessages<DraftApprenticeshipDeletedEvent>
    {
        public Task Handle(DraftApprenticeshipDeletedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class PaymentOrderChangedEventHandler : GenericHandler, IHandleMessages<PaymentOrderChangedEvent>
    {
        public Task Handle(PaymentOrderChangedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DraftApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<DraftApprenticeshipCreatedEvent>
    {
        public Task Handle(DraftApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }
    class DraftApprenticeshipUpdatedEventHandler : GenericHandler, IHandleMessages<DraftApprenticeshipUpdatedEvent>
    {
        public Task Handle(DraftApprenticeshipUpdatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }
}
