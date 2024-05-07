using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Types.UnitTests.Configuration;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ReservationsClientApiConfigurationTests
{
    private const string NonStubBase = "https://somehost.gov.uk";

    [Test]
    public void EffectiveApiBaseUrl_UseStub_ShouldBeSetToStub()
    {
        var config = CreateConfiguration(true);

        Assert.That(config.EffectiveApiBaseUrl, Is.EqualTo(ReservationsClientApiConfiguration.StubBase));
    }

    [Test]
    public void EffectiveApiBaseUrl_DoNotUseStub_ShouldBeSetToConfiguredApi()
    {
        var config = CreateConfiguration(false);

        Assert.That(config.EffectiveApiBaseUrl, Is.EqualTo(NonStubBase));
    }

    private static ReservationsClientApiConfiguration CreateConfiguration(bool useStub)
    {
        return new ReservationsClientApiConfiguration
        {
            UseStub = useStub,
            ApiBaseUrl = NonStubBase
        };
    }
}