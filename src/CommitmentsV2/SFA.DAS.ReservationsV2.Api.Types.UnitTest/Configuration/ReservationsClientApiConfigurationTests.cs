using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types.UnitTests.Configuration
{
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
        public void EffectiveApiBaseUrl_DoNotUseStub_ShouldBeSetToConfigurtedApi()
        {
            var config = CreateConfiguration(false);

            Assert.That(config.EffectiveApiBaseUrl, Is.EqualTo(NonStubBase));
        }

        private ReservationsClientApiConfiguration CreateConfiguration(bool useStub)
        {
            return new ReservationsClientApiConfiguration
            {
                UseStub = useStub,
                ApiBaseUrl = NonStubBase
            };
        }
    }
}
