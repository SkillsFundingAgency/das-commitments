using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Validation.CocApprovals.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class CocApprovalStatusServiceTestsForLastName
{
    private Mock<ILogger<CocApprovalStatusService>> _loggerMock;
    private Mock<IOverlapCheckService> _overlapCheckServiceMock;

    private Mock<IAcademicYearDateProvider> _academicYearDateProviderMock;
    private Mock<ICocApprovalStartDateValidationRules> _cocApprovalStartDateValidationRulesMock;
    private CocApprovalStatusService _service;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CocApprovalStatusService>>();
        _overlapCheckServiceMock = new Mock<IOverlapCheckService>();
        _academicYearDateProviderMock = new Mock<IAcademicYearDateProvider>();
        _cocApprovalStartDateValidationRulesMock = new Mock<ICocApprovalStartDateValidationRules>();
        _service = new CocApprovalStatusService(_overlapCheckServiceMock.Object, _cocApprovalStartDateValidationRulesMock.Object, _academicYearDateProviderMock.Object, _loggerMock.Object);
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
        var course = new Course();

        await _service.DetermineCocUpdateStatuses(updates, apprenticeship, course);

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
        var course = new Course();

        await _service.DetermineCocUpdateStatuses(updates, apprenticeship, course);

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
            LastName = "Lewi"
        };
        var course = new Course();

        var results = await _service.DetermineCocUpdateStatuses(updates, apprenticeship, course);

        results.Should().HaveCount(1);
        results[0].Status.Should().Be(CocApprovalItemStatus.AutoApproved);
    }
}