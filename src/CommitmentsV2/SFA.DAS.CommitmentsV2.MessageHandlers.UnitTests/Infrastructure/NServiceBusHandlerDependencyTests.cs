using FluentAssertions;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.TestHelpers.NServiceBus;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.Infrastructure;

[TestFixture]
public class NServiceBusHandlerDependencyTests
{
    [Test]
    public void NServiceBus_handlers_do_not_inject_IMessageSession_or_IEndpointInstance()
    {
        var handlerAssemblies = new[]
        {
            typeof(StoreLearningHistoryCommandHandler).Assembly,
            typeof(LearnerDataUpdatedEventHandler).Assembly
        };

        var violations = NServiceBusHandlerDependencyValidator.GetViolations(handlerAssemblies);

        violations.Should().BeEmpty(
            "NServiceBus rejects handlers that inject IMessageSession or IEndpointInstance and the endpoint will fail to start. " +
            "Use IMessageHandlerContext.Send or IMessageHandlerContext. Publish inside Handle instead.{0}{1}",
            Environment.NewLine,
            string.Join(Environment.NewLine, violations));
    }
}
