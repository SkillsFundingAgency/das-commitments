using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

public class NotifyProviderServiceTests
{
    private Mock<ILogger<NotifyProviderService>> _loggerMock;
    private Mock<IMessageSession> _messageSessionMock;
    private Mock<CommitmentsV2Configuration> _configurationMock;
    private NotifyProviderService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<NotifyProviderService>>();
        _messageSessionMock = new Mock<IMessageSession>();
        _configurationMock = new Mock<CommitmentsV2Configuration>();
        _service = new NotifyProviderService(_messageSessionMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task VerifyProviderNotification()
    {
        await _service.NotifyProvider(123, "456", "TestTemplate");

        _messageSessionMock.Verify(x => x.Send(It.Is<SendEmailToProviderCommand>(cmd =>
            cmd.ProviderId == 123 &&
            cmd.Template == "TestTemplate" &&
            cmd.Tokens["url"] == $"{_configurationMock.Object.ProviderCommitmentsBaseUrl}/123/apprentices/456"), It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task VerifyProviderNotificationWithEmployerName()
    {
        await _service.NotifyProvider(123, "456", "TestTemplate", "TestEmployer");

        _messageSessionMock.Verify(x => x.Send(It.Is<SendEmailToProviderCommand>(cmd =>
            cmd.ProviderId == 123 &&
            cmd.Template == "TestTemplate" &&
            cmd.Tokens["url"] == $"{_configurationMock.Object.ProviderCommitmentsBaseUrl}/123/apprentices/456" &&
            cmd.Tokens["employer"] == "TestEmployer"), It.IsAny<SendOptions>()), Times.Once);
    }
}