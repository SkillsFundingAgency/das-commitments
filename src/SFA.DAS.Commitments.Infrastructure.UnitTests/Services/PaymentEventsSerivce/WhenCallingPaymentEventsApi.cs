using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;
using NUnit.Framework;

using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.PaymentEventsSerivce
{
    [TestFixture]
    public class WhenCallingPaymentEventsApi
    {
        private Infrastructure.Services.PaymentEventsService _sut;

        private Mock<IPaymentsEventsApiClient> _paymentEventsApi;

        [SetUp]
        public void SetUp()
        {
            _paymentEventsApi = new Mock<IPaymentsEventsApiClient>();
            _sut = new Infrastructure.Services.PaymentEventsService(_paymentEventsApi.Object, new Infrastructure.Services.PaymentEventMapper(), Mock.Of<ILog>());
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

        [Test]
        public async Task WhenCallingPaymentService()
        {
            _paymentEventsApi.Setup(m => m.GetDataLockEvents(0, null, null, 0L, 1))
                .ReturnsAsync(new PageOfResults<DataLockEvent>
                                  {
                                      PageNumber = 1,
                                      TotalNumberOfPages = 1,
                                      Items = new DataLockEvent[0]
                                  });
            _sut.RetryWaitTimeInSeconds = 0;

            await _sut.GetDataLockEvents(2);
            _paymentEventsApi.Verify(m => m.GetDataLockEvents(2, null, null, 0L, 1), Times.Once);
        }
    }
}
