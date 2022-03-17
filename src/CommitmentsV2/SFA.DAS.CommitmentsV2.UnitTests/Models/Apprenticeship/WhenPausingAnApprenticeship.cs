using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    public class WhenPausingAnApprenticeship
    {
        private WhenPausingAnApprenticeshipFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenPausingAnApprenticeshipFixture(PaymentStatus.Active);
        }

        [Test]
        public void ThenPaymentStatusShouldBeSetToPaused()
        {
            _fixture.PauseApprenticeship();

            _fixture.Apprenticeship.PaymentStatus.Should().Be(PaymentStatus.Paused);
        }

        [Test]
        public void ThenPauseDateShouldBeSet()
        {
            _fixture.PauseApprenticeship();

            _fixture.Apprenticeship.PauseDate.Should().Be(_fixture.Now);
        }

        [Test]
        public void ThenShouldPublishApprenticeshipPausedEvent()
        {
            _fixture.PauseApprenticeship();

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<ApprenticeshipPausedEvent>()
                .Single()
                .Should()
                .Match<ApprenticeshipPausedEvent>(e =>
                    e.ApprenticeshipId == _fixture.Apprenticeship.Id);
        }

        [Test]
        public void AndPaymentStatusIsAlreadyNotActive_ThenExceptionIsThrown()
        {
            _fixture = new WhenPausingAnApprenticeshipFixture(PaymentStatus.Paused);

            Assert.Throws<DomainException>(() => _fixture.PauseApprenticeship() );
        }
    }

    public class WhenPausingAnApprenticeshipFixture
    {
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; set; }
        public CommitmentsV2.Models.Cohort Cohort { get; set; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
        public DateTime Now { get; set; }
        public DateTime? PauseDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Party Party { get; set; }
        public UserInfo UserInfo { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public WhenPausingAnApprenticeshipFixture(PaymentStatus paymentStatus)
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Now = DateTime.UtcNow;

            CurrentDateTime = new Mock<ICurrentDateTime>();
            CurrentDateTime.Setup(x => x.UtcNow).Returns(Now);
            PauseDate = Now;
            EndDate = Now;
            UnitOfWorkContext = new UnitOfWorkContext();

            Cohort = new CommitmentsV2.Models.Cohort()
               .Set(c => c.Id, 111)
               .Set(c => c.EmployerAccountId, 222)
               .Set(c => c.ProviderId, 333);

            UserInfo = fixture.Create<UserInfo>();

            Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                .With(s => s.Cohort, Cohort)
                .With(s => s.PaymentStatus, paymentStatus)
                .With(s => s.PauseDate, PauseDate)
                .Without(s => s.EndDate)
                .Without(s => s.DataLockStatus)
                .Without(s => s.EpaOrg)
                .Without(s => s.ApprenticeshipUpdate)
                .Without(s => s.Continuation)
                .Without(s => s.PreviousApprenticeship)
                .Without(s => s.FlexibleEmployment)
                .Create();

        }

        public void PauseApprenticeship()
        {
            Apprenticeship.PauseApprenticeship(CurrentDateTime.Object, Party, UserInfo);
        }
    }
}
