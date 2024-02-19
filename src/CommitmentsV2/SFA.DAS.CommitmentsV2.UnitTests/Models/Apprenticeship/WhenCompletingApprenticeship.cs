using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    [Parallelizable]
    [TestFixture]
    public class WhenCompletingApprenticeship
    {
        private WhenCompletingApprenticeshipFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new WhenCompletingApprenticeshipFixture();
        }

        [TestCase(ApprenticeshipStatus.Live, false)]
        [TestCase(ApprenticeshipStatus.Completed, true)]
        [TestCase(ApprenticeshipStatus.Paused, false)]
        [TestCase(ApprenticeshipStatus.Stopped, false)]
        [TestCase(ApprenticeshipStatus.WaitingToStart, true)]
        [TestCase(ApprenticeshipStatus.Unknown, true)]
        public void ThenThrowsExceptionBasedOnApprenticeshipStatus(ApprenticeshipStatus status, bool isExceptionExpected)
        {
            _fixture.WithApprenticeshipStatus(status);

            if (isExceptionExpected)
            {
                Assert.Throws<InvalidOperationException>(() => _fixture.Act());
            }
            else
            {
                _fixture.Act();
            }
        }

        [Test]
        public void ThenVerifyChangeTracking()
        {
            _fixture.WithApprenticeshipStatus(ApprenticeshipStatus.Live);

            _fixture.Act();

            _fixture.VerifyApprenticeshipTracking();
        }

        [Test]
        public void ThenPaymentStatusIsUpdated()
        {
            _fixture.WithApprenticeshipStatus(ApprenticeshipStatus.Live);

            _fixture.Act();

            _fixture.VerifyPaymentStatusChanged();
        }

        [Test]
        public void ThenCompletionDateIsUpdated()
        {
            _fixture.WithApprenticeshipStatus(ApprenticeshipStatus.Live);

            _fixture.Act();

            _fixture.VerifyCompletionDateChanged();
        }

        [Test]
        public void ThenApprenticeshipCompletedEventIsPublished()
        {
            _fixture.WithApprenticeshipStatus(ApprenticeshipStatus.Live);

            _fixture.Act();

            _fixture.VerifyApprenticeshipCompletedEventIsPublished();
        }
    }

    public class WhenCompletingApprenticeshipFixture
    {
        private UnitOfWorkContext _unitOfWorkContext;
        private CommitmentsV2.Models.Apprenticeship _apprenticeship;
        private CommitmentsV2.Models.Cohort _cohort;
        private DateTime _completionDate;
        private Fixture _fixture;

        public WhenCompletingApprenticeshipFixture()
        {
            _fixture = new Fixture();
            _completionDate = _fixture.Create<DateTime>();
            _unitOfWorkContext = new UnitOfWorkContext();
            _cohort = new CommitmentsV2.Models.Cohort
            {
                Id = 234,
                EmployerAccountId = 234,
                ProviderId = 234
            };
            _apprenticeship = new CommitmentsV2.Models.Apprenticeship
            {
                Id = 64,
                Cohort = _cohort,
                StartDate = new DateTime(2020,1,1)
            };
        }

        public CommitmentsV2.Models.Apprenticeship Act()
        {
            _apprenticeship.Complete(_completionDate);
            return _apprenticeship;
        }

        public WhenCompletingApprenticeshipFixture WithApprenticeshipStatus(ApprenticeshipStatus status)
        {
            switch (status)
            {
                case ApprenticeshipStatus.Live:
                    _apprenticeship.StartDate = _completionDate.AddMonths(-6);
                    _apprenticeship.PaymentStatus = PaymentStatus.Active;
                    break;
                case ApprenticeshipStatus.Stopped:
                    _apprenticeship.PaymentStatus = PaymentStatus.Withdrawn;
                    break;
                case ApprenticeshipStatus.WaitingToStart:
                    _apprenticeship.StartDate = _completionDate.AddMonths(6);
                    _apprenticeship.PaymentStatus = PaymentStatus.Active;
                    break;
                case ApprenticeshipStatus.Paused:
                    _apprenticeship.PaymentStatus = PaymentStatus.Paused;
                    break;
                case ApprenticeshipStatus.Completed:
                    _apprenticeship.PaymentStatus = PaymentStatus.Completed;
                    break;
            }
            return this;
        }

        public void VerifyPaymentStatusChanged()
        {
            Assert.That(_apprenticeship.PaymentStatus, Is.EqualTo(PaymentStatus.Completed));
        }


        public void VerifyCompletionDateChanged()
        {
            Assert.That(_apprenticeship.CompletionDate, Is.EqualTo(_completionDate));
        }

        public void VerifyApprenticeshipCompletedEventIsPublished()
        {
            var apprenticeshipCompletedEvent = _unitOfWorkContext
                .GetEvents()
                .OfType<ApprenticeshipCompletedEvent>()
                .First(x => 
                    x.CompletionDate == _completionDate &&
                    x.ApprenticeshipId == _apprenticeship.Id);

            Assert.That(apprenticeshipCompletedEvent, Is.Not.Null);
        }

        public void VerifyApprenticeshipTracking()
        {
            Assert.That(_unitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType == nameof(CommitmentsV2.Models.Apprenticeship)
                                                                                && @event.ProviderId == _apprenticeship.Cohort.ProviderId
                                                                                && @event.EmployerAccountId == _apprenticeship.Cohort.EmployerAccountId), Is.Not.Null);
        }
    }
}