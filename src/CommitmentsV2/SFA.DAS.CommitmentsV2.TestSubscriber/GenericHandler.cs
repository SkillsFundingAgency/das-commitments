using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.TestSubscriber;

internal class GenericHandler 
{
    protected static Task Log(object message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Received message id:{context.MessageId} type:{message.GetType().FullName} received:{DateTime.Now:T} Content: {JsonConvert.SerializeObject(message)}");
        return Task.CompletedTask;
    }
}

internal class ApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipCreatedEvent>
{
    public Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class ApprenticeshipPausedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipPausedEvent>
{
    public Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class ApprenticeshipResumedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipResumedEvent>
{
    public Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class ApprenticeshipStoppedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipStoppedEvent>
{
    public Task Handle(ApprenticeshipStoppedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class ApprenticeshipStopDateChangedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipStopDateChangedEvent>
{
    public Task Handle(ApprenticeshipStopDateChangedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class ApprenticeshipUpdatedApprovedEventHandler : GenericHandler, IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
{
    public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class DataLockTriageApprovedEventHandler : GenericHandler, IHandleMessages<DataLockTriageApprovedEvent>
{
    public Task Handle(DataLockTriageApprovedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class DraftApprenticeshipDeletedEventHandler : GenericHandler, IHandleMessages<DraftApprenticeshipDeletedEvent>
{
    public Task Handle(DraftApprenticeshipDeletedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class PaymentOrderChangedEventHandler : GenericHandler, IHandleMessages<PaymentOrderChangedEvent>
{
    public Task Handle(PaymentOrderChangedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class DraftApprenticeshipCreatedEventHandler : GenericHandler, IHandleMessages<DraftApprenticeshipCreatedEvent>
{
    public Task Handle(DraftApprenticeshipCreatedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}

internal class DraftApprenticeshipUpdatedEventHandler : GenericHandler, IHandleMessages<DraftApprenticeshipUpdatedEvent>
{
    public Task Handle(DraftApprenticeshipUpdatedEvent message, IMessageHandlerContext context)
    {
        return Log(message, context);
    }
}