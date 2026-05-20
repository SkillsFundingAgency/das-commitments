using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class LearnerWithdrawnEventHandlerTests
    {
        public LearnerWithdrawnEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new LearnerWithdrawnEventHandlerTestsFixture();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeship_StopDateAndCodeAreUpdated()
        {
            await _fixture.Handle();
            _fixture.VerifyStopDateIsAssignedCorrectly();
            _fixture.VerifyWithdrawnReasonCodeIsAssignedCorrectly();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedOnNonApprenticeship_Exception_IsThrown()
        {
            _fixture.SetApprenticeshipIdTo(999);
            var func = () => _fixture.Handle();
            await func.Should().ThrowAsync<Exception>();
            _fixture.VerifyHasError();
        }

        public class LearnerWithdrawnEventHandlerTestsFixture
        {
            private LearnerWithdrawnEventHandler _handler;
            private LearnerWithdrawnEvent _event;
            public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private FakeLogger<LearnerWithdrawnEventHandler> _logger;
            private FakeApprenticeship _apprenticeship;
            private Cohort _cohort;

            public LearnerWithdrawnEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
                _logger = new FakeLogger<LearnerWithdrawnEventHandler>();

                _handler = new LearnerWithdrawnEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object), _logger);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _event = autoFixture.Create<LearnerWithdrawnEvent>();

                _cohort = new Cohort() { Id = 1 };

                _apprenticeship = new FakeApprenticeship { Id = _event.ApprenticeshipId, CommitmentId = 1 };
                _dbContext.Object.Apprenticeships.Add(_apprenticeship);

                _cohort.Apprenticeships.Add(_apprenticeship);
                _dbContext.Object.Cohorts.Add(_cohort);
                _dbContext.Object.SaveChanges();
            }

            public LearnerWithdrawnEventHandlerTestsFixture SetApprenticeshipIdTo(long id)
            {
                _event.ApprenticeshipId = id;
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            public void VerifyStopDateIsAssignedCorrectly()
            {
                _apprenticeship.StopDate.Should().Be(_event.WithdrawnDate);
            }

            public void VerifyWithdrawnReasonCodeIsAssignedCorrectly()
            {
                _apprenticeship.WithdrawnReasonCode.Should().Be(_event.WithdrawnReasonCode);
            }

            public void VerifyHasError()
            {
                Assert.That(_logger.HasErrors, Is.True);
            }
        }

        private class FakeApprenticeship : Apprenticeship
        {
        }
    }
}
