namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TestEventHandler() : IHandleMessages<TestEvent>
{
    public Task Handle(TestEvent message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}


public class TestEvent
{
    public int? Id { get; set; }
}