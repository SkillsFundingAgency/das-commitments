using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship;

public class WhenFreezingPayments
{
    private WhenFreezingPaymentsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new WhenFreezingPaymentsFixture();
    }

    [Test]
    public void ThenPaymentFreezeDateShouldBeSet()
    {
        _fixture.FreezePayments();

        _fixture.Apprenticeship.PaymentFreezeDate.Should().Be(_fixture.Now.Date);
    }

    [Test]
    public void ThenFreezePaymentsReasonShouldBeSet()
    {
        _fixture.FreezePayments();

        _fixture.Apprenticeship.FreezePaymentsReason.Should().Be(FreezePaymentsReason.LearnerOnBreak);
    }

    [Test]
    public void ThenFreezeStatusShouldBeTrue()
    {
        _fixture.FreezePayments();

        _fixture.Apprenticeship.FreezeStatus.Should().BeTrue();
    }

    [Test]
    public void ThenPaymentStatusShouldRemainActive()
    {
        _fixture.FreezePayments();

        _fixture.Apprenticeship.PaymentStatus.Should().Be(PaymentStatus.Active);
        _fixture.Apprenticeship.PauseDate.Should().BeNull();
    }

    [Test]
    public void ThenShouldPublishApprenticeshipPausedEvent()
    {
        _fixture.FreezePayments();

        _fixture.UnitOfWorkContext.GetEvents()
            .OfType<ApprenticeshipPausedEvent>()
            .Single()
            .ApprenticeshipId.Should().Be(_fixture.Apprenticeship.Id);
    }

    [Test]
    public void WhenPaymentsAreAlreadyFrozen_ThenExceptionIsThrown()
    {
        _fixture = new WhenFreezingPaymentsFixture(alreadyFrozen: true);

        Assert.Throws<DomainException>(() => _fixture.FreezePayments());
    }

    [Test]
    public void WhenApprenticeshipIsNotLive_ThenExceptionIsThrown()
    {
        _fixture = new WhenFreezingPaymentsFixture(paymentStatus: PaymentStatus.Withdrawn);

        Assert.Throws<DomainException>(() => _fixture.FreezePayments());
    }

    public class WhenFreezingPaymentsFixture
    {
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; set; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
        public DateTime Now { get; set; }
        public Party Party { get; set; }
        public UserInfo UserInfo { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public WhenFreezingPaymentsFixture(
            PaymentStatus paymentStatus = PaymentStatus.Active,
            bool alreadyFrozen = false)
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Now = DateTime.UtcNow;
            CurrentDateTime = new Mock<ICurrentDateTime>();
            CurrentDateTime.Setup(x => x.UtcNow).Returns(Now);
            UnitOfWorkContext = new UnitOfWorkContext();
            Party = Party.Employer;
            UserInfo = fixture.Create<UserInfo>();

            var cohort = new CommitmentsV2.Models.Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333);

            Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                .With(s => s.Cohort, cohort)
                .With(s => s.PaymentStatus, paymentStatus)
                .With(s => s.StartDate, Now.AddMonths(-2))
                .With(s => s.PaymentFreezeDate, alreadyFrozen ? Now.Date.AddDays(-1) : null)
                .With(s => s.FreezePaymentsReason, alreadyFrozen ? FreezePaymentsReason.LearnerWithdrawn : null)
                .Without(s => s.PauseDate)
                .Without(s => s.EndDate)
                .Without(s => s.DataLockStatus)
                .Without(s => s.EpaOrg)
                .Without(s => s.ApprenticeshipUpdate)
                .Without(s => s.Continuation)
                .Without(s => s.PreviousApprenticeship)
                .Without(s => s.ApprenticeshipConfirmationStatus)
                .Create();
        }

        public void FreezePayments()
        {
            Apprenticeship.FreezePayments(
                CurrentDateTime.Object,
                Party,
                UserInfo,
                FreezePaymentsReason.LearnerOnBreak);
        }
    }
}
