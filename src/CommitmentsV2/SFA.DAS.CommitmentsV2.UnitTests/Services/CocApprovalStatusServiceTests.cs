using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class CocApprovalStatusServiceTests
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _service = new CocApprovalStatusService(_loggerMock.Object);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldThrow_WhenUpdatesIsNull()
    {
        Action act = () => _service.DetermineCocUpdateStatuses(null, new Apprenticeship());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("updates");
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldThrow_WhenApprenticeshipIsNull()
    {
        Action act = () => _service.DetermineCocUpdateStatuses(new CocUpdates(), null);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("apprenticeship");
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldReturnEmpty_WhenNoTnpFieldsPresent()
    {
        var updates = new CocUpdates();
        var apprenticeship = new Apprenticeship { Cost = 1000 };

        var result = _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().BeEmpty();
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldLogInformation_WhenTnpFieldsPresent()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 100, New = 90 }
        };

        var apprenticeship = new Apprenticeship { Cost = 1000 };

        _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Change of TNP1 or TNP2 detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldReturnPending_WhenOverallCourseCostRemainsTheSame()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 100, New = 80 },
            TNP2 = new CocUpdate<int> { Old = 200, New = 220 }
        };

        var apprenticeship = new Apprenticeship { Cost = 300 };

        var result = _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Status == CocApprovalItemStatus.Pending);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldReturnPending_WhenCostIncreases()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 100, New = 200 },
            TNP2 = new CocUpdate<int> { Old = 102, New = 202 }
        };

        var apprenticeship = new Apprenticeship { Cost = 202 };

        var result = _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().HaveCount(2);
        result[0].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[0].Field.Should().Be(CocChangeField.TNP1);
        result[1].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[1].Field.Should().Be(CocChangeField.TNP2);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldReturnPending_WhenCostDecreases()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 100, New = 95 },
            TNP2 = new CocUpdate<int> { Old = 102, New = 100 }
        };

        var apprenticeship = new Apprenticeship { Cost = 202 };

        var result = _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().HaveCount(2);
        result[0].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[0].Field.Should().Be(CocChangeField.TNP1);
        result[1].Status.Should().Be(CocApprovalItemStatus.Pending);
        result[1].Field.Should().Be(CocChangeField.TNP2);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldLogWarning_WhenOldCostDoesNotMatchApprenticeshipCost()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 10, New = 5 },
            TNP2 = new CocUpdate<int> { Old = 20, New = 15 }
        };

        var apprenticeship = new Apprenticeship { Cost = 500 };

        _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) =>
                    o.ToString().Contains("Old total cost from changes does not match apprenticeship cost")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldReturnAutoRejected_WhenTotalCost_ExceedsMaximumTotalTrainingCost()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 100, New = 20000 },
            TNP2 = new CocUpdate<int> { Old = 102, New = 81000 }
        };

        var apprenticeship = new Apprenticeship { Cost = 202 };

        var result = _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().HaveCount(2);
        result.Where(r => r.Field == CocChangeField.TNP1).First().Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result.Where(r => r.Field == CocChangeField.TNP2).First().Status.Should().Be(CocApprovalItemStatus.AutoRejected);
    }

    [Test]
    public void DetermineCocUpdateStatuses_ShouldReturnAutoRejected_WhenTNP1_IsZero()
    {
        var updates = new CocUpdates
        {
            TNP1 = new CocUpdate<int> { Old = 100, New = 0 },
            TNP2 = new CocUpdate<int> { Old = 102, New = 81000 }
        };

        var apprenticeship = new Apprenticeship { Cost = 202 };

        var result = _service.DetermineCocUpdateStatuses(updates, apprenticeship);

        result.Should().HaveCount(2);
        result[0].Field.Should().Be(CocChangeField.TNP1);
        result[0].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
        result[1].Field.Should().Be(CocChangeField.TNP2);
        result[1].Status.Should().Be(CocApprovalItemStatus.AutoRejected);
    }
}