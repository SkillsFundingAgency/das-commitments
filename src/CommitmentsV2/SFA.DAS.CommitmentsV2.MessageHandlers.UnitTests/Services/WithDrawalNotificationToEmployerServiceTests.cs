using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.Services;
using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.Services;

public class WithDrawalNotificationToEmployerServiceTests
{
    [Test]
    public async Task When_SendingNotification_ThenShouldSendEmail()
    {
        var fixture = new WithDrawalNotificationToEmployerServiceTestsFixture();
        await fixture.SendNotification();
        fixture._service.Verify(x => x.Encode(It.IsAny<long>(), It.IsAny<EncodingType>()), Times.AtLeastOnce);
        fixture.VerifyEmailSentToEmployer();
    }

    [Test]
    public async Task When_SendingNotification_ThenShouldNotSendEmail()
    {
        var fixture = new WithDrawalNotificationToEmployerServiceTestsFixture();
        await fixture.SendNotificationWithNoMatchingApprenticeship();
        fixture.VerifyEmailNotSentToEmployer();
    }

    public class WithDrawalNotificationToEmployerServiceTestsFixture
    {
        public IWithDrawalNotificationToEmployerService _sut;
        private ProviderCommitmentsDbContext _dbContext;
        public Mock<IEncodingService> _service;
        private Mock<CommitmentsV2Configuration> _configuration;
        private Mock<ILogger<WithDrawalNotificationToEmployerService>> _logger;
        private Mock<LearnerWithdrawalNotificationEvent> _event;
        public Mock<IMessageHandlerContext> _context;
        public Mock<IPipelineContext> _pipelineContext;
        private readonly long _employerEncodedAccountId;
        private Cohort _cohort;
        private Apprenticeship _apprenticeship;

        private readonly Fixture _autoFixture;

        private static CommitmentsV2Configuration commitmentsV2Configuration;

        public WithDrawalNotificationToEmployerServiceTestsFixture()
        {
            _autoFixture = new Fixture();
            _service = new Mock<IEncodingService>();
            _logger = new Mock<ILogger<WithDrawalNotificationToEmployerService>>();
            _configuration = new Mock<CommitmentsV2Configuration>();
            _event = new Mock<LearnerWithdrawalNotificationEvent>();
            _context = new Mock<IMessageHandlerContext>();
            _pipelineContext = _context.As<IPipelineContext>();
            commitmentsV2Configuration = new CommitmentsV2Configuration()
            {
                ProviderCommitmentsBaseUrl = "https://approvals.environmentname-pas.apprenticeships.education.gov.uk/"
            };

            _employerEncodedAccountId = _autoFixture.Create<long>();

            var account = new Account(1, "", "", "", DateTime.UtcNow);

            _cohort = new Cohort
            {
                EmployerAccountId = _employerEncodedAccountId,
                AccountLegalEntity = new AccountLegalEntity(account, 1, 2, "", "", "Test Name", OrganisationType.CompaniesHouse,
            "", DateTime.UtcNow),
                Provider = new Provider() { Name = _autoFixture.Create<string>() },
                Reference = _autoFixture.Create<string>()
            };

            _event.Object.ApprenticeshipId = _autoFixture.Create<long>();
            _apprenticeship = new Apprenticeship
            {
                Id = _event.Object.ApprenticeshipId,
                Cohort = _cohort,
                CourseName = _autoFixture.Create<string>()
            };

            _service.Setup(x => x.Encode(It.IsAny<long>(), It.IsAny<EncodingType>())).Returns(_employerEncodedAccountId.ToString());

            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

            _dbContext = new ProviderCommitmentsDbContext(options);

            _dbContext.Apprenticeships.Add(_apprenticeship);

            _dbContext.SaveChanges();

            _sut = new WithDrawalNotificationToEmployerService(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _service.Object, _configuration.Object, _logger.Object);
        }

        public async Task SendNotification()
        {
            _sut.SendWithdrawalNotificationToEmployer(_event.Object, _context.Object).GetAwaiter().GetResult();
        }

        public async Task SendNotificationWithNoMatchingApprenticeship()
        {
            _event.Object.ApprenticeshipId = _autoFixture.Create<long>();
            _sut.SendWithdrawalNotificationToEmployer(_event.Object, _context.Object).GetAwaiter().GetResult();
        }

        public void VerifyEmailSentToEmployer()
        {
            _pipelineContext.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(c =>
                c.Tokens["provider_name"] == _cohort.Provider.Name &&
                c.Tokens["employer_name"] == _cohort.AccountLegalEntity.Name &&
                c.Tokens["course_name"] == _apprenticeship.CourseName &&
                c.Tokens["url"] == $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}{_employerEncodedAccountId}/apprentices"
            ), It.IsAny<SendOptions>()));
        }

        public void VerifyEmailNotSentToEmployer()
        {
            _pipelineContext.Verify(x => x.Send(It.IsAny<SendEmailToEmployerCommand>(), It.IsAny<SendOptions>()), Times.Never);
        }
    }
}