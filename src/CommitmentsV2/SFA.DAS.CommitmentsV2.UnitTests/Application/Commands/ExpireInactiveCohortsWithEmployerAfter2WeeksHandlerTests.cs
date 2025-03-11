using SFA.DAS.CommitmentsV2.Application.Commands.ExpireInactiveCohortsWithEmployerAfter2Weeks;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;
[TestFixture]
public class ExpireInactiveCohortsWithEmployerAfter2WeeksHandlerTests
{
    public ExpireInactiveCohortsWithEmployerAfter2WeeksHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ExpireInactiveCohortsWithEmployerAfter2WeeksHandlerTestsFixture();
    }

    [Test]
    public async Task When_HandlingCommand_CohortIsSentToOtherParty()
    {
        await _fixture.Handle();
        _fixture.VerifyCohortIsSentToOtherParty();
    }

    public class ExpireInactiveCohortsWithEmployerAfter2WeeksHandlerTestsFixture
    {
        private readonly ExpireInactiveCohortsWithEmployerAfter2WeeksHandler _handler;
        private readonly ExpireInactiveCohortsWithEmployerAfter2WeeksCommand _command;
        public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
        private readonly Mock<Cohort> _cohort;
        private readonly CommitmentsV2Configuration _configuration;
        private readonly Mock<ICurrentDateTime> _currentDateTime;

        public ExpireInactiveCohortsWithEmployerAfter2WeeksHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options)
            { CallBase = true };

            _currentDateTime = new Mock<ICurrentDateTime>();
            _currentDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

            _configuration = new CommitmentsV2Configuration
            {
                ExpireInactiveEmployerCohortImplementationDate = DateTime.UtcNow.AddDays(-50),
            };

            _handler = new ExpireInactiveCohortsWithEmployerAfter2WeeksHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object),
                _currentDateTime.Object,
                _configuration
                );

            _command = autoFixture.Create<ExpireInactiveCohortsWithEmployerAfter2WeeksCommand>();

            _cohort = new Mock<Cohort>();
            _cohort.Setup(x => x.WithParty).Returns(Party.Employer);
            _cohort.Setup(x =>
                x.SendToOtherParty(Party.Employer, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()));

            _cohort.Object.LastAction = LastAction.Amend;
            _cohort.Object.LastUpdatedOn = DateTime.UtcNow.AddDays(-15);
            _cohort.Object.IsDraft = false;

            _dbContext
                .Setup(context => context.Cohorts)
                .ReturnsDbSet(new List<Cohort> { _cohort.Object });

            _dbContext.Object.SaveChanges();
        }

        public ExpireInactiveCohortsWithEmployerAfter2WeeksHandlerTestsFixture SetUpExceptionInQuery()
        {
            _dbContext.Setup(x => x.Cohorts).Throws<NullReferenceException>();
            return this;
        }

        public async Task Handle()
        {
            await _handler.Handle(_command, CancellationToken.None);
        }

        public void VerifyCohortIsSentToOtherParty()
        {
            _cohort.Verify(x => x.SendToOtherParty(Party.Employer, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}
