using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.PatchApprenticeshipPayments;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class PatchApprenticeshipPaymentsCommandHandlerTests
{
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<IAuthenticationService> _authenticationService;
    private Mock<ICurrentDateTime> _currentDateTime;
    private Mock<IMessageSession> _messageSession;
    private UnitOfWorkContext _unitOfWorkContext;
    private PatchApprenticeshipPaymentsCommandHandler _handler;

    [SetUp]
    public void Init()
    {
        _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        _authenticationService = new Mock<IAuthenticationService>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        _messageSession = new Mock<IMessageSession>();
        _unitOfWorkContext = new UnitOfWorkContext();

        _handler = new PatchApprenticeshipPaymentsCommandHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
            _currentDateTime.Object,
            _authenticationService.Object,
            _messageSession.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task Handle_WhenPaymentFreezeDateProvided_SetsPaymentFreezeDateAndReason()
    {
        var apprenticeship = await SetupApprenticeship();
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);

        await _handler.Handle(new PatchApprenticeshipPaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            PaymentFreezeDate = DateTime.UtcNow.Date,
            FreezePaymentsReason = FreezePaymentsReason.LearnerOnBreak,
            UserInfo = new UserInfo()
        }, CancellationToken.None);

        var updated = await _dbContext.Apprenticeships.FindAsync(apprenticeship.Id);
        updated.PaymentFreezeDate.Should().Be(_currentDateTime.Object.UtcNow.Date);
        updated.FreezePaymentsReason.Should().Be(FreezePaymentsReason.LearnerOnBreak);
        updated.FreezeStatus.Should().BeTrue();
        updated.PaymentStatus.Should().Be(PaymentStatus.Active);

        _messageSession.Verify(x => x.Send(
            It.Is<StoreLearningHistoryCommand>(c =>
                c.ApprenticeshipId == apprenticeship.Id &&
                c.Source == LearningSourceType.ApprovalAPI &&
                c.ChangeType == LearningChangeType.ManualUpdate &&
                c.AppliedDate == _currentDateTime.Object.UtcNow.Date &&
                c.Description == "Payments paused - Learner is on a break"),
            It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenPaymentFreezeDateCleared_ClearsPaymentFreezeDateAndReason()
    {
        var apprenticeship = await SetupApprenticeship(frozen: true);
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);

        await _handler.Handle(new PatchApprenticeshipPaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            PaymentFreezeDate = null,
            UserInfo = new UserInfo()
        }, CancellationToken.None);

        var updated = await _dbContext.Apprenticeships.FindAsync(apprenticeship.Id);
        updated.PaymentFreezeDate.Should().BeNull();
        updated.FreezePaymentsReason.Should().BeNull();
        updated.FreezeStatus.Should().BeFalse();

        _messageSession.Verify(x => x.Send(
            It.Is<StoreLearningHistoryCommand>(c =>
                c.ApprenticeshipId == apprenticeship.Id &&
                c.Source == LearningSourceType.ApprovalAPI &&
                c.ChangeType == LearningChangeType.ManualUpdate &&
                c.AppliedDate == _currentDateTime.Object.UtcNow &&
                c.Description == "Payments resumed"),
            It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenFreezePaymentsReasonMissing_ThrowsDomainException()
    {
        var apprenticeship = await SetupApprenticeship();
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);

        var exception = Assert.ThrowsAsync<DomainException>(() => _handler.Handle(new PatchApprenticeshipPaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            PaymentFreezeDate = DateTime.UtcNow.Date,
            UserInfo = new UserInfo()
        }, CancellationToken.None));

        exception.DomainErrors.Should().ContainEquivalentOf(new
        {
            PropertyName = nameof(PatchApprenticeshipPaymentsCommand.FreezePaymentsReason),
            ErrorMessage = "A reason for pausing payments must be provided"
        });

        _messageSession.Verify(x => x.Send(It.IsAny<StoreLearningHistoryCommand>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [TestCase(Party.Provider)]
    [TestCase(Party.TransferSender)]
    [TestCase(Party.None)]
    public void Handle_WhenFreezingAndPartyIsNotEmployer_ThrowsDomainException(Party party)
    {
        _authenticationService.Setup(a => a.GetUserParty()).Returns(party);

        var exception = Assert.ThrowsAsync<DomainException>(() => _handler.Handle(new PatchApprenticeshipPaymentsCommand
        {
            ApprenticeshipId = 1,
            PaymentFreezeDate = DateTime.UtcNow.Date,
            FreezePaymentsReason = FreezePaymentsReason.LearnerOnBreak,
            UserInfo = new UserInfo()
        }, CancellationToken.None));

        exception.DomainErrors.Should().ContainEquivalentOf(new
        {
            ErrorMessage = $"Only employers are allowed to freeze payments - {party} is invalid"
        });
    }

    [Test]
    public async Task Handle_WhenPartyProvidedAsEmployer_UsesCommandPartyInsteadOfAuthenticationService()
    {
        var apprenticeship = await SetupApprenticeship();
        _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Provider);

        await _handler.Handle(new PatchApprenticeshipPaymentsCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            PaymentFreezeDate = DateTime.UtcNow.Date,
            FreezePaymentsReason = FreezePaymentsReason.LearnerOnBreak,
            UserInfo = new UserInfo(),
            Party = Party.Employer
        }, CancellationToken.None);

        var updated = await _dbContext.Apprenticeships.FindAsync(apprenticeship.Id);
        updated.FreezeStatus.Should().BeTrue();
    }

    [TestCase(Party.Provider)]
    public void Handle_WhenUnfreezingAndPartyIsNotEmployer_ThrowsDomainException(Party party)
    {
        _authenticationService.Setup(a => a.GetUserParty()).Returns(party);

        var exception = Assert.ThrowsAsync<DomainException>(() => _handler.Handle(new PatchApprenticeshipPaymentsCommand
        {
            ApprenticeshipId = 1,
            PaymentFreezeDate = null,
            UserInfo = new UserInfo()
        }, CancellationToken.None));

        exception.DomainErrors.Should().ContainEquivalentOf(new
        {
            ErrorMessage = $"Only employers are allowed to unfreeze payments - {party} is invalid"
        });
    }

    private async Task<Apprenticeship> SetupApprenticeship(bool frozen = false)
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
