using SFA.DAS.CommitmentsV2.Application.Commands.FreezePayments;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class FreezePaymentsCommandHandlerTests
{
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<IAuthenticationService> _authenticationService;
    private Mock<ICurrentDateTime> _currentDateTime;
    private UnitOfWorkContext _unitOfWorkContext;
    private IRequestHandler<FreezePaymentsCommand> _handler;

    [SetUp]
    public void Init()
    {
        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        _authenticationService = new Mock<IAuthenticationService>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        _unitOfWorkContext = new UnitOfWorkContext();

        _handler = new FreezePaymentsCommandHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
            _currentDateTime.Object,
            _authenticationService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task Handle_WhenValid_SetsPaymentFreezeDateAndReason()
    {
        var apprenticeship = await SetupApprenticeship();
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);

        await _handler.Handle(new FreezePaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            FreezePaymentsReason = FreezePaymentsReason.LearnerOnBreak,
            UserInfo = new UserInfo()
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.Apprenticeships.FindAsync(apprenticeship.Id);
        updated.PaymentFreezeDate.Should().Be(_currentDateTime.Object.UtcNow.Date);
        updated.FreezePaymentsReason.Should().Be(FreezePaymentsReason.LearnerOnBreak);
        updated.FreezeStatus.Should().BeTrue();
        updated.PaymentStatus.Should().Be(PaymentStatus.Active);
    }

    [Test]
    public async Task Handle_WhenReasonMissing_ThrowsDomainException()
    {
        var apprenticeship = await SetupApprenticeship();
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);

        var exception = Assert.ThrowsAsync<DomainException>(() => _handler.Handle(new FreezePaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            UserInfo = new UserInfo()
        }, CancellationToken.None));

        exception.DomainErrors.Should().ContainEquivalentOf(new
        {
            PropertyName = nameof(FreezePaymentsCommand.FreezePaymentsReason),
            ErrorMessage = "A reason for pausing payments must be provided"
        });
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.TransferSender)]
    [TestCase(Party.None)]
    public void Handle_WhenPartyIsNotEmployer_ThrowsDomainException(Party party)
    {
        _authenticationService.Setup(a => a.GetUserParty()).Returns(party);

        var exception = Assert.ThrowsAsync<DomainException>(() => _handler.Handle(new FreezePaymentsCommand
        {
            ApprenticeshipId = 1,
            FreezePaymentsReason = FreezePaymentsReason.LearnerOnBreak,
            UserInfo = new UserInfo()
        }, CancellationToken.None));

        exception.DomainErrors.Should().ContainEquivalentOf(new
        {
            ErrorMessage = $"Only employers are allowed to freeze payments - {party} is invalid"
        });
    }

    private async Task<Apprenticeship> SetupApprenticeship()
    {
        var fixture = new Fixture();
        var apprenticeship = new Apprenticeship
        {
            Cohort = new Cohort
            {
                EmployerAccountId = fixture.Create<long>(),
                ProviderId = fixture.Create<long>(),
                AccountLegalEntity = new AccountLegalEntity()
            },
            PaymentStatus = PaymentStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-2)
        };

        _dbContext.Apprenticeships.Add(apprenticeship);
        await _dbContext.SaveChangesAsync();

        return apprenticeship;
    }
}
