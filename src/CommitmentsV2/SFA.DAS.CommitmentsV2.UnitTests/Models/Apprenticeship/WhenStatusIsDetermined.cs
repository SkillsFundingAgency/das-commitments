using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Types;
using PaymentStatus = SFA.DAS.CommitmentsV2.Types.PaymentStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    [TestFixture]
    [Parallelizable]

    public class WhenStatusIsDetermined
    {
        private ApprenticeshipStatusTestsFixture _fixture;
        
        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipStatusTestsFixture();
        }

        [Test]
        public void Future_Starts_Show_As_WaitingToStart()
        {
            _fixture.WithFutureStartDate().VerifyStatus(ApprenticeshipStatus.WaitingToStart);
        }

        [Test]
        public void Past_Starts_Show_As_Live()
        {
            _fixture.WithPastStartDate().VerifyStatus(ApprenticeshipStatus.Live);
        }

        [Test]
        public void Stopped_Apprenticeships_Show_As_Stopped()
        {
            _fixture.WithPaymentStatus(PaymentStatus.Withdrawn).VerifyStatus(ApprenticeshipStatus.Stopped);
        }

        [Test]
        public void Paused_Apprenticeships_Show_As_Paused()
        {
            _fixture.WithPaymentStatus(PaymentStatus.Paused).VerifyStatus(ApprenticeshipStatus.Paused);
        }

        [Test]
        public void Completed_Apprenticeships_Show_As_Completed()
        {
            _fixture.WithPaymentStatus(PaymentStatus.Completed).VerifyStatus(ApprenticeshipStatus.Completed);
        }

        [Test]
        public void All_PaymentStatus_Are_Mapped()
        {
            _fixture.VerifyAllPaymentStatusAreMappedToApprenticeshipStatus();
        }

        private class ApprenticeshipStatusTestsFixture
        {
            private readonly CommitmentsV2.Models.Apprenticeship _apprenticeship;

            public ApprenticeshipStatusTestsFixture()
            {
                _apprenticeship = new CommitmentsV2.Models.Apprenticeship();

                _apprenticeship = new CommitmentsV2.Models.Apprenticeship
                {
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    EndDate = DateTime.UtcNow.AddYears(1),
                    PaymentStatus = PaymentStatus.Active
                };
            }

            public ApprenticeshipStatusTestsFixture WithFutureStartDate()
            {
                _apprenticeship.StartDate = DateTime.UtcNow.AddMonths(1);
                _apprenticeship.EndDate = _apprenticeship.StartDate.Value.AddYears(1);
                return this;
            }

            public ApprenticeshipStatusTestsFixture WithPastStartDate()
            {
                _apprenticeship.StartDate = DateTime.UtcNow.AddMonths(-1);
                _apprenticeship.EndDate = _apprenticeship.StartDate.Value.AddYears(1);
                return this;
            }

            public ApprenticeshipStatusTestsFixture WithPaymentStatus(PaymentStatus paymentStatus)
            {
                _apprenticeship.PaymentStatus = paymentStatus;
                return this;
            }

            public void VerifyStatus(ApprenticeshipStatus expectedStatus)
            {
                Assert.AreEqual(expectedStatus, _apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow));
            }

            internal void VerifyAllPaymentStatusAreMappedToApprenticeshipStatus()
            {
                foreach (PaymentStatus paymentStatus in Enum.GetValues(typeof(PaymentStatus)))
                {
                    _apprenticeship.PaymentStatus = paymentStatus;
                    Assert.AreNotEqual(ApprenticeshipStatus.Unknown, _apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow), $"PaymentStatus : {paymentStatus.ToString()} is not mapped to Apprenticeship status");
                }
            }
        }
    }
}
