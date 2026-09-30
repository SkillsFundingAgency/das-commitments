using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class CocApprovalStatusServiceTestsForLastName
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;
    private Mock<IOverlapCheckService> _overlapCheckServiceMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _overlapCheckServiceMock = new Mock<IOverlapCheckService>();
        _overlapCheckServiceMock.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CourseDateRange>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OverlapCheckResult(false, false));

        _service = new CocApprovalStatusService(_overlapCheckServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldLogInformation_WhenLastNameFieldPresent()
    {
        var updates = new CocUpdates
        {
            Lastname = new CocUpdate<string>
            {
                Old = "Lewi",
                New = "Lewis"
            }
        };
        var apprenticeship = new Apprenticeship
        {
            LastName = "Lewi"
        };

        await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Change of Lastname detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldLogInformation_WhenLastnameOldValueIsNotEqualCurrentValue()
    {
        var updates = new CocUpdates
        {
            Lastname = new CocUpdate<string>
            {
                Old = "Dean",
                New = "Lewis"
            }
        };
        var apprenticeship = new Apprenticeship
        {
            LastName = "Lewi"
        };

        await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Old last name value from changes, does not match apprenticeship last name value")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DetermineCocUpdateStatuses_ShouldReturnAutoApproved_WhenNoErrors()
    {
        var updates = new CocUpdates
        {
            Lastname = new CocUpdate<string>
            {
                Old = "Lewi",
                New = "Lewis"
            }
        };
        var apprenticeship = new Apprenticeship
        {
            FirstName = "Lewi"
        };

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoApproved);
    }
}