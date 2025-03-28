using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers;
[TestFixture]
public class EmployerSendCohortCommandHandlerTests
{
    public EmployerSendCohortCommandHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new EmployerSendCohortCommandHandlerTestsFixture();
    }

    [Test]
    public async Task When_HandlingCommand_CohortIsSentToOtherParty()
    {
        await _fixture.Handle();
        _fixture.VerifyCohortIsSentToOtherParty();
    }

    [Test]
    public async Task When_HandlingCommand_CohortIsAlreadySentToOtherParty_Then_ShouldNotCallSendToOtherPartyAndLogWarning()
    {
        await _fixture.SetWithPartyToProvider().Handle();
        _fixture.VerifySendToOtherPartyIsNotCalled();
        _fixture.VerifyHasWarning();
    }

    [Test]
    public void Handle_WhenHandlingCommandAndItFails_ThenItShouldThrowAnExceptionAndLogIt()
    {
        Assert.ThrowsAsync<NullReferenceException>(() => _fixture.SetupNullMessage().Handle());
        _fixture.VerifyHasError();
    }

    public class EmployerSendCohortCommandHandlerTestsFixture
    {
        private EmployerSendCohortCommandHandler _handler;
        private EmployerSendCohortCommand _command;
        public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
        private Mock<IMessageHandlerContext> _messageHandlerContext;
        private FakeLogger<EmployerSendCohortCommandHandler> _logger;
        private Mock<Cohort> _cohort;

        public EmployerSendCohortCommandHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
            _logger = new FakeLogger<EmployerSendCohortCommandHandler>();

            _handler = new EmployerSendCohortCommandHandler(_logger,
                new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object));

            _messageHandlerContext = new Mock<IMessageHandlerContext>();

            _command = autoFixture.Create<EmployerSendCohortCommand>();

            _cohort = new Mock<Cohort>();
            _cohort.Setup(x => x.Id).Returns(_command.CohortId);
            _cohort.Setup(x => x.WithParty).Returns(Party.Employer);
            _cohort.Setup(x => x.IsApprovedByAllParties).Returns(false);
            _cohort.Setup(x =>
                x.SendToOtherParty(Party.Employer, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()));

            _dbContext
                .Setup(context => context.Cohorts)
                .ReturnsDbSet(new List<Cohort> { _cohort.Object });

            _dbContext.Object.SaveChanges();
        }

        public EmployerSendCohortCommandHandlerTestsFixture SetWithPartyToProvider()
        {
            _cohort.Setup(x => x.WithParty).Returns(Party.Provider);
            return this;
        }
        public EmployerSendCohortCommandHandlerTestsFixture SetupNullMessage()
        {
            _command = null;
            return this;
        }

        public async Task Handle()
        {
            await _handler.Handle(_command, _messageHandlerContext.Object);
        }

        public void VerifyCohortIsSentToOtherParty()
        {
            _cohort.Verify(x => x.SendToOtherParty(Party.Employer, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()), Times.Once);
        }

        public void VerifySendToOtherPartyIsNotCalled()
        {
            _cohort.Verify(x => x.SendToOtherParty(It.IsAny<Party>(), It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()), Times.Never);
        }

        public void VerifyHasError()
        {
            Assert.That(_logger.HasErrors, Is.True);
        }

        public void VerifyHasWarning()
        {
            Assert.That(_logger.HasWarnings, Is.True);
        }
    }
}

