using SFA.DAS.CommitmentsV2.Application.Commands.UnfreezePayments;
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
public class UnfreezePaymentsCommandHandlerTests
{
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<IAuthenticationService> _authenticationService;
    private Mock<ICurrentDateTime> _currentDateTime;
    private UnitOfWorkContext _unitOfWorkContext;
    private IRequestHandler<UnfreezePaymentsCommand> _handler;

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

        _handler = new UnfreezePaymentsCommandHandler(
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
    public async Task Handle_WhenValid_ClearsPaymentFreezeDateAndReason()
    {
        var apprenticeship = await SetupApprenticeship(frozen: true);
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);

        await _handler.Handle(new UnfreezePaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            UserInfo = new UserInfo()
        }, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.Apprenticeships.FindAsync(apprenticeship.Id);
        updated.PaymentFreezeDate.Should().BeNull();
        updated.FreezePaymentsReason.Should().BeNull();
        updated.FreezeStatus.Should().BeFalse();
    }

    [TestCase(Party.Provider)]
    public void Handle_WhenPartyIsNotEmployer_ThrowsDomainException(Party party)
    {
        _authenticationService.Setup(a => a.GetUserParty()).Returns(party);

        var exception = Assert.ThrowsAsync<DomainException>(() => _handler.Handle(new UnfreezePaymentsCommand
        {
            ApprenticeshipId = 1,
            UserInfo = new UserInfo()
        }, CancellationToken.None));

        exception.DomainErrors.Should().ContainEquivalentOf(new
        {
            ErrorMessage = $"Only employers are allowed to unfreeze payments - {party} is invalid"
        });
    }

    private async Task<Apprenticeship> SetupApprenticeship(bool frozen)
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
            StartDate = DateTime.UtcNow.AddMonths(-2),
            PaymentFreezeDate = frozen ? DateTime.UtcNow.Date.AddDays(-2) : null,
            FreezePaymentsReason = frozen ? FreezePaymentsReason.LearnerWithdrawn : null
        };

        _dbContext.Apprenticeships.Add(apprenticeship);
        await _dbContext.SaveChangesAsync();

        return apprenticeship;
    }
}
