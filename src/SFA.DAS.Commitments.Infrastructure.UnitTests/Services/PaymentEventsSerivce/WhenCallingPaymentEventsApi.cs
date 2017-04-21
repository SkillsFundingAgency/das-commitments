using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;
using NUnit.Framework;

using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.PaymentEventsSerivce
{
    [TestFixture]
    public class WhenCallingPaymentEventsApi
    {
        private Infrastructure.Services.PaymentEventsSerivce _sut;

        private Mock<IPaymentsEventsApiClient> _paymentEventsApi;

        [SetUp]
        public void SetUp()
        {
            _paymentEventsApi = new Mock<IPaymentsEventsApiClient>();
            _sut = new Infrastructure.Services.PaymentEventsSerivce(_paymentEventsApi.Object, new Infrastructure.Services.PaymentEventMapper(), Mock.Of<ILog>());
        }

        [Test]
        public async Task WhenGettingExceptionsFromApi()
        {
            _paymentEventsApi.Setup(m => m.GetDataLockEvents(0, null, null, 0L, 1)).Throws<Exception>();
            _sut.RetryWaitTimeInSeconds = 0;
            var result = await _sut.GetDataLockEvents();

            _paymentEventsApi.Verify(m => m.GetDataLockEvents(0, null, null, 0L, 1), Times.Exactly(3));
            result.Count().Should().Be(0);
        }
    }
}
