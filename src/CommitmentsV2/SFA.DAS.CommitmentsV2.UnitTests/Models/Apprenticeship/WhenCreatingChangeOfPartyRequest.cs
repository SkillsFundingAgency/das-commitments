using System;
using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    [TestFixture]
    public class WhenCreatingChangeOfPartyRequest
    {
        private WhenCreatingChangeOfPartyRequestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenCreatingChangeOfPartyRequestFixture();
        }

        [Test]
        public void ThenChangeOfPartyRequestIsCreated()
        {
            _fixture.CreateChangeOfPartyRequest();
            _fixture.VerifyResult();
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenApprenticeshipMustAlreadyBeStopped(PaymentStatus status)
        {
            _fixture
                .WithStatus(status)
                .CreateChangeOfPartyRequest();

            _fixture.VerifyException<DomainException>();
        }

        [TestCase("2019-06-01", true, Description = "Before stop")]
        [TestCase("2020-02-29", true, Description = "Day before stop")]
        [TestCase("2020-03-01", false, Description = "Day of stop")]
        [TestCase("2020-06-01", false, Description = "After stop")]        
        public void ThenStartDateMustBeOnOrAfterStopDate(DateTime? startDate, bool expectThrow)
        {
            _fixture
                .WithStartDate(startDate)
                .CreateChangeOfPartyRequest();

            if (expectThrow)
            {
                _fixture.VerifyException<DomainException>();
            }
            else
            {
                _fixture.VerifyResult();
            }
        }

        [TestCase(ChangeOfPartyRequestStatus.Pending, true)]
        [TestCase(ChangeOfPartyRequestStatus.Approved, true)]
        [TestCase(ChangeOfPartyRequestStatus.Rejected, false)]
        [TestCase(ChangeOfPartyRequestStatus.Withdrawn, false)]
        public void ThenApprenticeshipMustNotAlreadyHaveAPendingOrApprovedRequest(ChangeOfPartyRequestStatus status, bool expectThrow)
        {
            _fixture
                .WithExistingChangeOfPartyRequest(status)
                .CreateChangeOfPartyRequest();

            if (expectThrow)
            {
                _fixture.VerifyException<DomainException>();
            }
            else
            {
                _fixture.VerifyResult();
            }
        }

        private class WhenCreatingChangeOfPartyRequestFixture
        {
            public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; private set; }
            public PaymentStatus PaymentStatus { get; private set; }
            public DateTime? StopDate { get; private set; }
            public DateTime? StartDate { get; private set; }
            public List<CommitmentsV2.Models.ChangeOfPartyRequest> PreviousChangeOfPartyRequests { get; private set; }
            public CommitmentsV2.Models.ChangeOfPartyRequest Result { get; private set; }
            public UnitOfWorkContext UnitOfWorkContext { get; private set; }
            public Exception Exception { get; private set; }

            public WhenCreatingChangeOfPartyRequestFixture()
            {
                UnitOfWorkContext = new UnitOfWorkContext();

                PaymentStatus = PaymentStatus.Withdrawn;
                StopDate = new DateTime(2020, 03, 01);
                StartDate = StopDate.Value;
                PreviousChangeOfPartyRequests = new List<CommitmentsV2.Models.ChangeOfPartyRequest>();
            }

            public WhenCreatingChangeOfPartyRequestFixture WithStatus(PaymentStatus paymentStatus)
            {
                PaymentStatus = paymentStatus;
                StopDate = paymentStatus == PaymentStatus.Withdrawn ? new DateTime(2020, 03, 01) : default;
                return this;
            }

            public WhenCreatingChangeOfPartyRequestFixture WithStopDate(DateTime? stopDate)
            {
                StopDate = stopDate;
                return this;
            }

            public WhenCreatingChangeOfPartyRequestFixture WithStartDate(DateTime? startDate)
            {
                StartDate = startDate;
                return this;
            }

            public WhenCreatingChangeOfPartyRequestFixture WithExistingChangeOfPartyRequest(ChangeOfPartyRequestStatus status)
            {
                var request = new CommitmentsV2.Models.ChangeOfPartyRequest();

                var t = typeof(CommitmentsV2.Models.ChangeOfPartyRequest);
                t.GetProperty("Status").SetValue(request, status, null);

                PreviousChangeOfPartyRequests.Add(request);

                return this;
            }

            public void CreateChangeOfPartyRequest()
            {
                try
                {
                    Apprenticeship = new CommitmentsV2.Models.Apprenticeship
                    {
                        PaymentStatus = PaymentStatus,
                        StopDate = StopDate,
                        Cohort = new CommitmentsV2.Models.Cohort(),
                        ChangeOfPartyRequests = PreviousChangeOfPartyRequests
                    };

                    Result = Apprenticeship.CreateChangeOfPartyRequest(ChangeOfPartyRequestType.ChangeEmployer, Party.Provider, 1,
                        1000, StartDate, DateTime.UtcNow, null, null, null, new UserInfo(), DateTime.UtcNow);

                }
                catch (Exception e)
                {
                    Exception = e;
                }
            }

            public void VerifyResult()
            {
                Assert.That(Exception, Is.Null);
                Assert.That(Result, Is.Not.Null);
            }

            public void VerifyException<T>()
            {
                Assert.That(Exception, Is.Not.Null);
                Assert.That(Exception, Is.InstanceOf<T>());
            }
        }
    }
}
