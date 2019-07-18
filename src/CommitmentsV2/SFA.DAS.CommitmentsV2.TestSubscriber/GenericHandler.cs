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

    class ApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>
    {
        public Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipPausedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipPausedEvent>
    {
        public Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipResumedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipResumedEvent>
    {
        public Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipStoppedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipStoppedEvent>
    {
        public Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipStopDateChangedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipStopDateChangedEvent>
    {
        public Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class ApprenticeshipUpdatedApprovedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipUpdatedApprovedEvent>
    {
        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DataLockTriageApprovedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.DataLockTriageApprovedEvent>
    {
        public Task Handle(DataLockTriageApprovedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DraftApprenticeshipDeletedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.DraftApprenticeshipDeletedEvent>
    {
        public Task Handle(DraftApprenticeshipDeletedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class PaymentOrderChangedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.PaymentOrderChangedEvent>
    {
        public Task Handle(PaymentOrderChangedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DraftApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.DraftApprenticeshipCreatedEvent>
    {
        public Task Handle(DraftApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }

    class DraftApprenticeshipUpdatedEventHandler : GenericHandler, IHandleMessages<SFA.DAS.CommitmentsV2.Messages.Events.DraftApprenticeshipUpdatedEvent>
    {
        public Task Handle(DraftApprenticeshipUpdatedEvent message, IMessageHandlerContext context)
        {
            return Log(message, context);
        }
    }
}