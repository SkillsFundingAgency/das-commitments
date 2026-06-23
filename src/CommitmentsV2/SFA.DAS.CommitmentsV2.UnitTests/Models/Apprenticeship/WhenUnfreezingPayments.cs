using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship;

public class WhenUnfreezingPayments
{
    private WhenUnfreezingPaymentsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new WhenUnfreezingPaymentsFixture();
    }

    [Test]
    public void ThenPaymentFreezeDateShouldBeCleared()
    {
        _fixture.UnfreezePayments();

        _fixture.Apprenticeship.PaymentFreezeDate.Should().BeNull();
    }

    [Test]
    public void ThenFreezePaymentsReasonShouldBeCleared()
    {
        _fixture.UnfreezePayments();

        _fixture.Apprenticeship.FreezePaymentsReason.Should().BeNull();
    }

    [Test]
    public void ThenFreezeStatusShouldBeFalse()
    {
        _fixture.UnfreezePayments();

        _fixture.Apprenticeship.FreezeStatus.Should().BeFalse();
    }

    [Test]
    public void ThenShouldPublishApprenticeshipResumedEvent()
    {
        _fixture.UnfreezePayments();

        _fixture.UnitOfWorkContext.GetEvents()
            .OfType<ApprenticeshipResumedEvent>()
            .Single()
            .ApprenticeshipId.Should().Be(_fixture.Apprenticeship.Id);
    }

    [Test]
    public void WhenPaymentsAreNotFrozen_ThenExceptionIsThrown()
    {
        _fixture = new WhenUnfreezingPaymentsFixture(isFrozen: false);

        Assert.Throws<DomainException>(() => _fixture.UnfreezePayments());
    }

    public class WhenUnfreezingPaymentsFixture
    {
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; set; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
        public DateTime Now { get; set; }
        public Party Party { get; set; }
        public UserInfo UserInfo { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public WhenUnfreezingPaymentsFixture(bool isFrozen = true)
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
                .With(s => s.PaymentStatus, PaymentStatus.Active)
                .With(s => s.StartDate, Now.AddMonths(-2))
                .With(s => s.PaymentFreezeDate, isFrozen ? Now.Date.AddDays(-3) : null)
                .With(s => s.FreezePaymentsReason, isFrozen ? FreezePaymentsReason.ChangeToTrainingDetails : null)
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

        public void UnfreezePayments()
        {
            Apprenticeship.UnfreezePayments(CurrentDateTime.Object, Party, UserInfo);
        }
    }
}
