using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class CocManualApprovalsServiceTests
{
    private CocApprovalsConfiguration _configuration;
    private CocManualApprovalsService _service;

    [SetUp]
    public void Setup()
    {
        _configuration = new CocApprovalsConfiguration
        {
            UkPrns = new List<long> { 10001, 10002 },
            LockedManualFields = new List<string> { "StartDate", "Cost" }
        };

        _service = new CocManualApprovalsService(_configuration);
    }

    [Test]
    public void IsFieldLockedForProvider_ShouldReturnFalse_WhenUkPrnsIsNull()
    {
        _configuration.UkPrns = null;

        var result = _service.IsFieldLockedForProvider("StartDate", 10001);

        result.Should().BeFalse();
    }

    [Test]
    public void IsFieldLockedForProvider_ShouldReturnFalse_WhenProviderIsNotConfigured()
    {
        var result = _service.IsFieldLockedForProvider("StartDate", 99999);

        result.Should().BeFalse();
    }

    [Test]
    public void IsFieldLockedForProvider_ShouldReturnFalse_WhenLockedManualFieldsIsNull()
    {
        _configuration.LockedManualFields = null;

        var result = _service.IsFieldLockedForProvider("StartDate", 10001);

        result.Should().BeFalse();
    }

    [Test]
    public void IsFieldLockedForProvider_ShouldReturnTrue_WhenProviderAndFieldAreConfigured()
    {
        var result = _service.IsFieldLockedForProvider("StartDate", 10001);

        result.Should().BeTrue();
    }

    [Test]
    public void IsFieldLockedForProvider_ShouldReturnTrue_WhenFieldMatchDiffersByCase()
    {
        var result = _service.IsFieldLockedForProvider("startdate", 10001);

        result.Should().BeTrue();
    }

    [Test]
    public void IsFieldLockedForProvider_ShouldReturnFalse_WhenFieldIsNotConfigured()
    {
        var result = _service.IsFieldLockedForProvider("EndDate", 10001);

        result.Should().BeFalse();
    }
}
