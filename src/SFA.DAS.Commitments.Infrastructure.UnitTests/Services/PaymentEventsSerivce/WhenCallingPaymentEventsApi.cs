using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

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
        public void WhenGettingExceptionsFromApi()
        {
            _paymentEventsApi.Setup(m => m.GetDataLockEvents(0, null, null, 0L, 1)).Throws<Exception>();

            Func<Task<IEnumerable<DataLockStatus>>> act = async () => await _sut.GetDataLockEvents();

            act.ShouldThrow<Exception>();

            _paymentEventsApi.Verify(m => m.GetDataLockEvents(0, null, null, 0L, 1), Times.Exactly(4));
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

            await _sut.GetDataLockEvents(2);
            _paymentEventsApi.Verify(m => m.GetDataLockEvents(2, null, null, 0L, 1), Times.Once);
        }

        [Test]
        public void WhenGettingExceptionsFromApiFetchingSubmissionEvents()
        {
            _paymentEventsApi.Setup(m => m.GetSubmissionEvents(0, null, 0L, 1)).Throws<Exception>();

            Func<Task<PageOfResults<SubmissionEvent>>> act = async () => await _sut.GetSubmissionEvents();

            act.ShouldThrow<Exception>();

            _paymentEventsApi.Verify(m => m.GetSubmissionEvents(0, null, 0L, 1), Times.Exactly(4));
        }

        [Test]
        public async Task WhenCallingPaymentServiceToFetchSubmissionEvents()
        {
            _paymentEventsApi.Setup(m => m.GetSubmissionEvents(0, null, 0L, 1))
                .ReturnsAsync(new PageOfResults<SubmissionEvent>
                {
                    PageNumber = 1,
                    TotalNumberOfPages = 1,
                    Items = new SubmissionEvent[0]
                });

            await _sut.GetSubmissionEvents(2);
            _paymentEventsApi.Verify(m => m.GetSubmissionEvents(2, null, 0L, 1), Times.Once);
        }
    }
}
