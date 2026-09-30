using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AcknowledgeInvalidIlrChanges;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.AcknowledgeInvalidIlrChanges;

[TestFixture]
public class AcknowledgeInvalidIlrChangesCommandHandlerTests
{
    private AcknowledgeInvalidIlrChangesCommandHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new AcknowledgeInvalidIlrChangesCommandHandlerTestsFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task Handle_ThenDeleteSetsAcknowledgedAtAndByTogether()
    {
        await _fixture.Handle(deleteAlert: true);

        var request = _fixture.GetRequest();
        request.ProviderAcknowledgedAt.Should().Be(_fixture.Now);
        request.ProviderAcknowledgedBy.Should().Be("user-123");
    }

    [Test]
    public async Task Handle_ThenKeepLeavesAcknowledgedColumnsNull()
    {
        await _fixture.Handle(deleteAlert: false);

        var request = _fixture.GetRequest();
        request.ProviderAcknowledgedAt.Should().BeNull();
        request.ProviderAcknowledgedBy.Should().BeNull();
    }

    [Test]
    public async Task Handle_ThenThrowsWhenARadioIsUnanswered()
    {
        var act = () => _fixture.Handle(deleteAlert: null);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.DomainErrors.Should().ContainSingle(error =>
            error.PropertyName == "RequestSets[0].DeleteAlert" &&
            error.ErrorMessage == AcknowledgeInvalidIlrChangesCommandHandler.SelectDeleteMessage);
    }

    [Test]
    public async Task Handle_ThenThrowsWhenProviderDoesNotOwnTheApprenticeship()
    {
        var act = () => _fixture.Handle(deleteAlert: true, providerId: 999);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Handle_ThenDeleteSetsAcknowledgedAtAndByTogetherForEmployerRejected()
    {
        _fixture.SetItemStatus(CocApprovalItemStatus.EmployerRejected);

        await _fixture.Handle(deleteAlert: true, itemStatus: CocApprovalItemStatus.EmployerRejected);

        var request = _fixture.GetRequest();
        request.ProviderAcknowledgedAt.Should().Be(_fixture.Now);
        request.ProviderAcknowledgedBy.Should().Be("user-123");
    }

    [Test]
    public async Task Handle_ThenDoesNotAcknowledgeEmployerRejectedWhenFilteringAutoRejected()
    {
        _fixture.SetItemStatus(CocApprovalItemStatus.EmployerRejected);

        await _fixture.Handle(deleteAlert: true);

        _fixture.GetRequest().ProviderAcknowledgedAt.Should().BeNull();
        _fixture.GetRequest().ProviderAcknowledgedBy.Should().BeNull();
    }

    private sealed class AcknowledgeInvalidIlrChangesCommandHandlerTestsFixture : IDisposable
    {
        private readonly AcknowledgeInvalidIlrChangesCommandHandler _handler;
        private readonly ProviderCommitmentsDbContext _db;
        private readonly Guid _requestId = Guid.NewGuid();

        public DateTime Now { get; } = new(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

        public AcknowledgeInvalidIlrChangesCommandHandlerTestsFixture()
        {
            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            SeedData();

            var currentDateTime = new Mock<ICurrentDateTime>();
            currentDateTime.Setup(x => x.UtcNow).Returns(Now);

            _handler = new AcknowledgeInvalidIlrChangesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => _db),
                currentDateTime.Object);
        }

        public Task Handle(bool? deleteAlert, long providerId = 333, CocApprovalItemStatus itemStatus = CocApprovalItemStatus.AutoRejected)
        {
            return _handler.Handle(new AcknowledgeInvalidIlrChangesCommand
            {
                ApprenticeshipId = 1001,
                ProviderId = providerId,
                UserInfo = new UserInfo { UserId = "user-123" },
                ItemStatus = itemStatus,
                Acknowledgements =
                [
                    new InvalidIlrChangeAcknowledgement
                    {
                        ApprovalRequestId = _requestId,
                        DeleteAlert = deleteAlert
                    }
                ]
            }, CancellationToken.None);
        }

        public ApprovalRequest GetRequest()
        {
            return _db.ApprovalRequests.Single(request => request.Id == _requestId);
        }

        public void SetItemStatus(CocApprovalItemStatus itemStatus)
        {
            var item = _db.ApprovalRequests.Single(request => request.Id == _requestId).Items.Single();
            item.Status = itemStatus;
            _db.SaveChanges();
        }

        private void SeedData()
        {
            var cohort = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.EmployerAccountId, 222);

            var apprenticeship = new Apprenticeship
            {
                Id = 1001,
                Cohort = cohort,
                PaymentStatus = PaymentStatus.Active
            };

            apprenticeship.ApprovalRequests = new List<ApprovalRequest>
            {
                new()
                {
                    Id = _requestId,
                    ApprenticeshipId = 1001,
                    Status = CocApprovalResultStatus.Complete,
                    Items = new List<ApprovalFieldRequest>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Field = "TNP1",
                            Status = CocApprovalItemStatus.AutoRejected
                        }
                    }
                }
            };

            _db.Cohorts.Add(cohort);
            _db.Apprenticeships.Add(apprenticeship);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
