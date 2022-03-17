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
using System.Linq;
using AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    public class WhenEditingEndDate
    {
        private WhenEditingEndDateFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenEditingEndDateFixture();
        }

        [Test]
        public void ThenShouldPublishApprenticeshipUpdatedApprovedEvent()
        {
            var newEndDate = _fixture.EndDate.AddDays(1);
            _fixture.UpdateEndDate(newEndDate);

            _fixture.UnitOfWorkContext.GetEvents()
                .OfType<ApprenticeshipUpdatedApprovedEvent>()
                .Single()
                .Should()
                .Match<ApprenticeshipUpdatedApprovedEvent>(e => e.ApprenticeshipId == _fixture.Apprenticeship.Id &&
                e.ApprovedOn == _fixture.CurrentDateTime.Object.UtcNow&&
                e.StartDate == _fixture.StartDate &&
                e.EndDate == newEndDate &&
                e.TrainingType == _fixture.Apprenticeship.ProgrammeType &&
                e.TrainingCode == _fixture.Apprenticeship.CourseCode&&
                e.Uln == _fixture.Apprenticeship.Uln);
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn)]
        public void AndApprenticeshipStatus_IsNotCompleted_ThenShouldThrowDomainException(PaymentStatus status)
        {
            _fixture.SetPaymentStatus(status);
            var newEndDate = _fixture.EndDate.AddDays(1);
            Assert.Throws<DomainException>(() => _fixture.UpdateEndDate(newEndDate));
        }

        [Test]
        public void ThenEndDateShouldBeUpdated()
        {
            var newEndDate = _fixture.EndDate.AddDays(1); 
            _fixture.UpdateEndDate(newEndDate);
            _fixture.Apprenticeship.EndDate.Should().Be(newEndDate);
        }

        [Test]
        public void AndEndDate_IsAfter_CompletionDate_ThenShouldThrowDomainException()
        {
            var newEndDate = _fixture.CompletionDate.AddDays(1);
            Assert.Throws<DomainException>(() => _fixture.UpdateEndDate(newEndDate));
        }

        [Test]
        public void AndEndDate_IsBefore_StartDate_ThenShouldThrowDomainException()
        {
            var newEndDate = _fixture.StartDate.AddDays(-1);
            Assert.Throws<DomainException>(() => _fixture.UpdateEndDate(newEndDate));
        }

        [Test]
        public void ThenVerifyTracking()
        {
            _fixture.UpdateEndDate(_fixture.EndDate);
            _fixture.VerifyApprenticeshipTracking(); 
        }
    }

    public class WhenEditingEndDateFixture
    {
        public DateTime EndDate { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public CommitmentsV2.Models.Cohort Cohort { get; set; }
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; set; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
        public Party Party { get; set; }
        public UserInfo UserInfo { get; set; }
        public DateTime CompletionDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Now { get; set; }

        public WhenEditingEndDateFixture()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Now = EndDate = DateTime.UtcNow;
            CompletionDate = EndDate.AddDays(10);
            StartDate = EndDate.AddDays(-10);

            UnitOfWorkContext = new UnitOfWorkContext();

            Cohort = new CommitmentsV2.Models.Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333);
        
            CurrentDateTime = new Mock<ICurrentDateTime>();
            CurrentDateTime.Setup(x => x.UtcNow).Returns(Now);

            UserInfo = fixture.Create<UserInfo>();

            Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                .With(s => s.Cohort, Cohort)
                .With(s => s.PaymentStatus, PaymentStatus.Completed)
                .With(s => s.EndDate, EndDate)
                .With(s => s.CompletionDate, CompletionDate)
                .With(s => s.StartDate, StartDate)
                .Without(s => s.DataLockStatus)
                .Without(s => s.EpaOrg)
                .Without(s => s.ApprenticeshipUpdate)
                .Without(s => s.Continuation)
                .Without(s => s.PreviousApprenticeship)
                .Without(s => s.FlexibleEmployment)
                .Create();
        }

        public WhenEditingEndDateFixture SetPaymentStatus(PaymentStatus status)
        {
            Apprenticeship.PaymentStatus = status;
            return this;
        }

        public WhenEditingEndDateFixture SetCompletionDate(DateTime completionDate)
        {
            Apprenticeship.CompletionDate = completionDate;
            return this;
        }

        public void VerifyApprenticeshipTracking()
        {
            Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType == nameof(Apprenticeship)
                                                                                && @event.ProviderId == Apprenticeship.Cohort.ProviderId
                                                                                && @event.EmployerAccountId == Apprenticeship.Cohort.EmployerAccountId));
        }

        public void UpdateEndDate(DateTime date)
        {
            Apprenticeship.EditEndDateOfCompletedRecord(date, CurrentDateTime.Object, Party, UserInfo);
        }
    }
}

