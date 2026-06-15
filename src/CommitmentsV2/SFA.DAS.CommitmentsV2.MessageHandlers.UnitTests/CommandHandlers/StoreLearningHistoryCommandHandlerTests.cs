using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers;

[TestFixture]
public class StoreLearningHistoryCommandHandlerTests
{
    public StoreLearningHistoryCommandHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new StoreLearningHistoryCommandHandlerTestsFixture();
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory()
    {
        await _fixture.Handle();
        _fixture.VerifyHistoryWasSaved();
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapAccountId()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.AccountId.Should().Be(_fixture.Account.Id);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapApprenticeshipId()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.ApprenticeshipId.Should().Be(_fixture.Apprenticeship.Id);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapAppliedDate()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.AppliedDate.Should().Be(_fixture.command.AppliedDate);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapChangeType()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.ChangeType.Should().Be((byte)_fixture.command.ChangeType);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapSource()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.Source.Should().Be((byte)_fixture.command.Source);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapDescription()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.Description.Should().Be(_fixture.command.Description);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapLearningKey()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.LearningKey.Should().Be(_fixture.command.LearningKey);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapEmployerName()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.EmployerName.Should().Be(_fixture.AccountLegalEntity.Name);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapProviderName()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.ProviderName.Should().Be(_fixture.Provider.Name);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapUkprn()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.UKPRN.Should().Be(_fixture.Provider.UkPrn);
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapLearnerName()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.LearnerName.Should().Be($"{_fixture.Apprenticeship.FirstName} {_fixture.Apprenticeship.LastName}");
    }

    [Test]
    public async Task When_HandlingCommand_StoreLearningHistory_Should_MapCreated()
    {
        await _fixture.Handle();

        var history = _fixture._dbContext.Object.LearningChangeHistory.FirstOrDefault();
        history.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task When_HandlingCommandFails_ThenItShouldLogException()
    {
        var act = () => _fixture.SetupNullMessage().Handle();

        await act.Should().ThrowAsync<ArgumentNullException>();

        _fixture.VerifyHasError();
    }

    [Test]
    public async Task When_HandlingCommandWithInvalidMessageId_ThenItShouldLogError()
    {
        await _fixture.SetupLearningChangeHistory().Handle();
        _fixture.VerifyLogInformation("has already been processed");
    }

    public class StoreLearningHistoryCommandHandlerTestsFixture
    {
        private StoreLearningHistoryCommandHandler _handler;
        public StoreLearningHistoryCommand command { get; set; }
        public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
        private Mock<IMessageHandlerContext> _messageHandlerContext;
        private FakeLogger<StoreLearningHistoryCommandHandler> _logger;
        public Guid MessageId { get; set; }

        public Apprenticeship Apprenticeship { get; set; }
        public Account Account { get; set; }

        public Provider Provider { get; set; }

        public AccountLegalEntity AccountLegalEntity { get; set; }

        public StoreLearningHistoryCommandHandlerTestsFixture()
        {
            MessageId = Guid.NewGuid();
            Provider = new Provider()
            {
                UkPrn = 12345,
                Name = "Test Provider"
            };

            Account = new Account(1, "", "", "", DateTime.UtcNow);

            AccountLegalEntity = new AccountLegalEntity(Account,
                  1,
                  0,
                  "",
                  "HasedId", "Test Employer",
                  OrganisationType.PublicBodies,
                  "",
                  DateTime.UtcNow);
            var cohort = new Cohort()
            {
                Id = 1,
                Provider = Provider,
                ProviderId = Provider.UkPrn,
                AccountLegalEntity = AccountLegalEntity,
                EmployerAccountId = 1,
                AccountLegalEntityId = AccountLegalEntity.Id,
            };

            Apprenticeship = new Apprenticeship()
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Apprentice",
                CommitmentId = cohort.Id,
                Cohort = cohort,
            };

            _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
            _logger = new FakeLogger<StoreLearningHistoryCommandHandler>();

            _handler = new StoreLearningHistoryCommandHandler(_logger,
                new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object));

            _messageHandlerContext = new Mock<IMessageHandlerContext>();
            _messageHandlerContext.Setup(c => c.MessageId).Returns(MessageId.ToString());

            command = new StoreLearningHistoryCommand() { ApprenticeshipId = Apprenticeship.Id, AppliedDate = DateTime.UtcNow, ChangeType = LearningChangeType.AutoApproved, Description = "Test Description", LearningKey = Guid.NewGuid(), Source = LearningSourceType.ApprovalAPI };

            _dbContext.Setup(c => c.Providers).ReturnsDbSet(new List<Provider> { Provider });

            _dbContext.Setup(c => c.AccountLegalEntities).ReturnsDbSet(new List<AccountLegalEntity> { AccountLegalEntity });
            _dbContext.Setup(c => c.Cohorts).ReturnsDbSet(new List<Cohort> { cohort });
            _dbContext
               .Setup(context => context.Apprenticeships)
               .ReturnsDbSet(new List<Apprenticeship> { Apprenticeship });
            _dbContext.Setup(c => c.Accounts).ReturnsDbSet(new List<Account> { Account });
        }

        public StoreLearningHistoryCommandHandlerTestsFixture SetupLearningChangeHistory()
        {
            _dbContext.Setup(c => c.LearningChangeHistory).ReturnsDbSet(new List<LearningChangeHistory>()
            {
                 new(){
                     Id = MessageId,
                     ApprenticeshipId = 1,
                     AccountId = 1,
                     AppliedDate = DateTime.UtcNow,
                     ChangeType = (byte)LearningChangeType.AutoApproved,
                     Description = "Test Description",
                     LearningKey = Guid.NewGuid(),
                     Source = (byte)LearningSourceType.ApprovalAPI,
                     Created = DateTime.UtcNow,
                     EmployerName = "Test Employer",
                     ProviderName = "Test Provider",
                     UKPRN = 12345,
                     LearnerName = "Test Apprentice"
                 }
            });

            return this;
        }

        public StoreLearningHistoryCommandHandlerTestsFixture SetupNullMessage()
        {
            command = null;
            return this;
        }

        public async Task Handle()
        {
            await _handler.Handle(command, _messageHandlerContext.Object);
        }

        public void VerifyHistoryWasSaved()
        {
            _dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        public void VerifyHasError()
        {
            _logger.HasErrors.Should().BeTrue();
        }

        public void VerifyLogInformation(string message)
        {
            _logger.LogMessages.Should().Contain(m => m.logLevel == LogLevel.Information && m.message.Contains(message));
        }
    }
}