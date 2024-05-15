using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    [TestFixture]
    [Parallelizable]
    public class WhenUpdatingCompletionDate
    {
        private WhenUpdatingCompletionDateFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenUpdatingCompletionDateFixture();
        }

        [Test]
        public void ThenShouldPublishApprenticeshipCompletionDateUpdatedEvent()
        {
            _fixture.Apprenticeship.UpdateCompletionDate(_fixture.Now);

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<ApprenticeshipCompletionDateUpdatedEvent>()
                .Single()
                .Should()
                .Match<ApprenticeshipCompletionDateUpdatedEvent>(e => e.ApprenticeshipId == _fixture.Apprenticeship.Id &&
                    e.CompletionDate == _fixture.Now);
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn)]
        public void AndApprenticeshipStatus_IsNotCompleted_ThenShouldThrowDomainException(PaymentStatus status)
        {
            _fixture.SetPaymentStatus(status);
            Assert.Throws<DomainException>(()=> _fixture.Apprenticeship.UpdateCompletionDate(_fixture.Now));
        }

        [Test]
        public void ThenCompletionDateShouldBeUpdated()
        {
            _fixture.Apprenticeship.UpdateCompletionDate(_fixture.Now);
            _fixture.Apprenticeship.CompletionDate.Should().Be(_fixture.Now);
        }

        [Test]
        public void ThenVerifyTracking()
        {
            _fixture.Apprenticeship.UpdateCompletionDate(_fixture.Now);
            _fixture.VerifyApprenticeshipTracking();;
        }
    }

    public class WhenUpdatingCompletionDateFixture
    {
        public DateTime Now { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public CommitmentsV2.Models.Cohort Cohort { get; set; }
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; set; }

        public WhenUpdatingCompletionDateFixture()
        {
            Now = DateTime.UtcNow;
            UnitOfWorkContext = new UnitOfWorkContext();
            
            Cohort = new CommitmentsV2.Models.Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333);
            Apprenticeship = new CommitmentsV2.Models.Apprenticeship { Cohort = Cohort, PaymentStatus = PaymentStatus.Completed};
        }

        public WhenUpdatingCompletionDateFixture SetPaymentStatus(PaymentStatus status)
        {
            Apprenticeship.PaymentStatus = status;
            return this;
        }

        public void VerifyApprenticeshipTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType == nameof(Apprenticeship) 
                                                                                && @event.ProviderId == Apprenticeship.Cohort.ProviderId 
                                                                                && @event.EmployerAccountId == Apprenticeship.Cohort.EmployerAccountId), Is.Not.Null);
        }
    }
}