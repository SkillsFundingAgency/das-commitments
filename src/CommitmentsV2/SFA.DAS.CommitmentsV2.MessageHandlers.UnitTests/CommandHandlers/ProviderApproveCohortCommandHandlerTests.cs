using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    public class ProviderApproveCohortCommandHandlerTests
    {
        public ProviderApproveCohortCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ProviderApproveCohortCommandHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingCommand_ApproveCohort()
        {
            await _fixture.Handle();
            _fixture.VerifyCohortApproval();
        }

        [Test]
        public async Task When_HandlingCommand_ApproveCohort_Again_ShouldNotCallCohortApprovalAndShouldLogWarning()
        {
            await _fixture.AddAlreadyApprovedByProvider().Handle();
            _fixture.VerifyCohortApprovalWasNotCalled();
            _fixture.VerifyHasWarning();
        }

        [Test]
        public void Handle_WhenHandlingCommandAndItFails_ThenItShouldThrowAnExceptionAndLogIt()
        {
            Assert.ThrowsAsync<NullReferenceException>( () =>_fixture.SetupNullMessage().Handle());
            _fixture.VerifyHasError();
        }

        public class ProviderApproveCohortCommandHandlerTestsFixture
        {
            private ProviderApproveCohortCommandHandler _handler;
            private ProviderApproveCohortCommand _command;
            public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private FakeLogger<ProviderApproveCohortCommandHandler> _logger;
            private Mock<Cohort> _cohort;

            public ProviderApproveCohortCommandHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
                _logger = new FakeLogger<ProviderApproveCohortCommandHandler>();

                _handler = new ProviderApproveCohortCommandHandler(_logger,
                    new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object));

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _command = autoFixture.Create<ProviderApproveCohortCommand>();

                _cohort = new Mock<Cohort>();
                _cohort.Setup(x => x.Id).Returns(_command.CohortId);
                _cohort.Setup(x => x.Approvals).Returns(Party.None);
                _cohort.Setup(x => x.IsApprovedByAllParties).Returns(false);

                _cohort.Setup(x =>
                    x.Approve(Party.Provider, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()));
               
                _dbContext.Object.Cohorts.Add(_cohort.Object);
                _dbContext.Object.SaveChanges();
            }

            public ProviderApproveCohortCommandHandlerTestsFixture AddAlreadyApprovedByProvider()
            {
                _cohort.Setup(x => x.Approvals).Returns(Party.Provider);
                return this;
            }

            public ProviderApproveCohortCommandHandlerTestsFixture SetupNullMessage()
            {
                _command = null;
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_command, _messageHandlerContext.Object);
            }

            public void VerifyCohortApproval()
            {
                _cohort.Verify(x=> x.Approve(Party.Provider, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()), Times.Once);
            }

            public void VerifyCohortApprovalWasNotCalled()
            {
                _cohort.Verify(x => x.Approve(It.IsAny<Party>(), It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()), Times.Never);
            }

            public void VerifyHasError()
            {
                Assert.IsTrue(_logger.HasErrors);
            }

            public void VerifyHasWarning()
            {
                Assert.IsTrue(_logger.HasWarnings);
            }
        }
    }
}
