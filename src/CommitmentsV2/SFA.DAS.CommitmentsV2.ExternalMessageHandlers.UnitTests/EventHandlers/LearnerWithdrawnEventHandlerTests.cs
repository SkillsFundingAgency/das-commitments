using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
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
            _fixture.VerifyStopApprenticeshipCommandIsSent();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeship_StoreLearnerHistoryCommand_IsPublished()
        {
            await _fixture.Handle();
            _fixture.VerifyStopApprenticeshipCommandIsSent();
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
            private Mock<IMediator> _mediator;
            private FakeLogger<LearnerWithdrawnEventHandler> _logger;
            private FakeApprenticeship _apprenticeship;
            private Cohort _cohort;

            public LearnerWithdrawnEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
                _logger = new FakeLogger<LearnerWithdrawnEventHandler>();
                _mediator = new Mock<IMediator>();

                _handler = new LearnerWithdrawnEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object), _mediator.Object, _logger);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _event = autoFixture.Create<LearnerWithdrawnEvent>();

                _cohort = new Cohort { Id = 1, EmployerAccountId = autoFixture.Create<long>() };

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

            public void VerifyStopApprenticeshipCommandIsSent()
            {
                _mediator.Verify(x => x.Send(It.Is<StopApprenticeshipCommand>(c =>
                    c.ApprenticeshipId == _event.ApprenticeshipId &&
                    c.AccountId == _cohort.EmployerAccountId &&
                    c.StopDate == _event.WithdrawnDate &&
                    c.StopSource == StopSource.Ilr &&
                    c.WithdrawnReasonCode == _event.WithdrawnReasonCode &&
                    c.LearningKey == _event.LearningKey &&
                    c.AppliedDate == _event.Created
                ), It.IsAny<CancellationToken>()), Times.Once);
            }
            
            public void VerifyHasError()
            {
                _logger.HasErrors.Should().BeTrue();
            }
        }

        private class FakeApprenticeship : Apprenticeship
        {
        }
    }
}
