using AutoFixture.Kernel;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprovalRequestAlertSeen;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class UpdateApprovalRequestAlertAcknowledgeCommandHandlerTests
{
    private UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture fixture;

    [SetUp]
    public void Arrange()
    {
        fixture = new UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture();
    }

    [Test]
    public async Task Handle_WhenHandlingCommand_ThenShouldUpdateEmployerAlertAcknowledge()
    {
        fixture.SetApprovalRequest().Handle();
        fixture.VerifyEmployerAcknowledgedAlert();
    }

    [Test]
    public async Task Handle_WhenHandlingCommand_ThenShouldUpdateEmployerMultipleAlertAcknowledge()
    {
        fixture.SetMultipleApprovalRequests().Handle();
        fixture.VerifyEmployerAcknowledgedAlert();
    }

    [Test]
    public async Task Handle_WhenHandlingCommand_ThenShouldThrowException()
    {
        var action = async () => fixture.SetApprovalRequest().SetCommandApprenticeshipId().Handle();
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Handle_WhenHandlingCommand_NoApprovalRequestsToUpdate_ShouldNotThrowException()
    {
        var action = async () => fixture.Handle();
        await action.Should().NotThrowAsync<UnauthorizedAccessException>();
    }
}

public class UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture
{
    public UpdateApprovalRequestAlertAcknowledgeCommand Command { get; set; }
    public Mock<ProviderCommitmentsDbContext> Db { get; set; }
    public IRequestHandler<UpdateApprovalRequestAlertAcknowledgeCommand> Handler { get; set; }
    public long ApprenticeshipId { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public long ApprenticeshipId2 { get; set; }
    public Guid ApprovalRequestId2 { get; set; }
    public long AccountId { get; set; }
    public long AccountLegalEntityId { get; private set; }
    public ApprovalRequest ApprovalRequest { get; private set; }
    public List<ApprovalFieldRequest> ApprovalFieldRequests { get; private set; }
    public Apprenticeship Apprenticeship { get; private set; }
    public Cohort Cohort { get; private set; }
    public Provider Provider { get; private set; }
    public AccountLegalEntity AccountLegalEntity { get; private set; }
    public Course Course { get; private set; }

    public GetApprovalRequestQuery Request;
    public GetApprovalRequestQueryResult Result;

    private readonly GetApprovalRequestQueryHandler _handler;
    private readonly ProviderCommitmentsDbContext _db;
    private Fixture _autoFixture;

    public UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture()
    {
        _autoFixture = new Fixture();
        _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _autoFixture.Customizations.Add(
            new TypeRelay(
                typeof(SFA.DAS.CommitmentsV2.Models.ApprenticeshipBase),
                typeof(Apprenticeship)));

        ApprenticeshipId = _autoFixture.Create<long>();
        AccountId = _autoFixture.Create<long>();
        ApprovalRequestId = _autoFixture.Create<Guid>();

        Provider = new Provider
        {
            UkPrn = _autoFixture.Create<long>(),
            Name = _autoFixture.Create<string>()
        };

        var account = new Account(1, "", "", "", DateTime.UtcNow);

        AccountLegalEntity = new AccountLegalEntity(account,
            AccountLegalEntityId,
            0,
            "",
            publicHashedId: _autoFixture.Create<string>(),
            _autoFixture.Create<string>(),
            OrganisationType.PublicBodies,
            "",
            DateTime.UtcNow);

        Cohort = new Cohort
        {
            Id = _autoFixture.CreateMany<long>().Last(),
            AccountLegalEntity = AccountLegalEntity,
            EmployerAccountId = AccountId,
            ProviderId = Provider.UkPrn,
            Provider = Provider,
            ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy
        };

        var courseCode = _autoFixture.Create<string>();

        Apprenticeship = new Apprenticeship
        {
            Id = ApprenticeshipId,
            CommitmentId = Cohort.Id,
            Cohort = Cohort,
            FirstName = _autoFixture.Create<string>(),
            LastName = _autoFixture.Create<string>(),
        };
        Command = new UpdateApprovalRequestAlertAcknowledgeCommand
        {
            ApprenticeshipId = ApprenticeshipId,
            AccountId = AccountId,
            ApprovalRequests = new List<UpdateApprovalRequestAlertAcknowledge>
        {
            new UpdateApprovalRequestAlertAcknowledge
            {
                ApprovalRequestId = ApprovalRequestId,
                 Acknowledged = true,
                UserInfo = new UserInfo
                {
                    UserId = Guid.NewGuid().ToString(),
                    UserDisplayName = "Test User"
                },
            }
        }
        };

        Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };

        Db.Object.Apprenticeships.Add(Apprenticeship);
        Db.Object.SaveChanges();
        Handler = new UpdateApprovalRequestAlertAcknowledgeCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), Mock.Of<ILogger<UpdateApprovalRequestAlertAcknowledgeCommandHandler>>());
    }

    public UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture SetMultipleApprovalRequests()
    {
        ApprovalRequestId2 = Guid.NewGuid();
        Db.Object.CocApprovalItems.Add(CreateAutoApprovedRow(ApprovalRequestId));
        Db.Object.CocApprovalItems.Add(CreateAutoApprovedRow(ApprovalRequestId2));
        Command.ApprovalRequests.Add(new UpdateApprovalRequestAlertAcknowledge
        {
            ApprovalRequestId = ApprovalRequestId2,
            Acknowledged = true,
            UserInfo = Command.ApprovalRequests[0].UserInfo
        });
        Db.Object.SaveChanges();

        return this;
    }

    public UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture SetApprovalRequest()
    {
        Db.Object.CocApprovalItems.Add(CreateAutoApprovedRow(ApprovalRequestId));
        PendingRowId = Guid.NewGuid();
        Db.Object.CocApprovalItems.Add(new CocApprovalItem
        {
            Id = PendingRowId,
            ApprenticeshipId = ApprenticeshipId,
            Status = CocApprovalItemStatus.Pending,
            Field = "Prices",
            Old = "{}",
            New = "{}",
            LearningKey = Guid.NewGuid(),
            LearningType = CocLearningType.Apprenticeship,
            UKPRN = "10000000",
            ULN = "1234567890"
        });
        Db.Object.SaveChanges();

        return this;
    }

    public Guid PendingRowId { get; private set; }

    private CocApprovalItem CreateAutoApprovedRow(Guid id)
    {
        return new CocApprovalItem
        {
            Id = id,
            ApprenticeshipId = ApprenticeshipId,
            Status = CocApprovalItemStatus.AutoApproved,
            Field = "Firstname",
            Old = "Ann",
            New = "Anne",
            LearningKey = Guid.NewGuid(),
            LearningType = CocLearningType.Apprenticeship,
            UKPRN = "10000000",
            ULN = "1234567890"
        };
    }

    public UpdateApprovalRequestAlertAcknowledgeCommandHandlerTestsFixture SetCommandApprenticeshipId()
    {
        Command.ApprenticeshipId = _autoFixture.Create<long>();
        return this;
    }

    public void VerifyEmployerAcknowledgedAlert()
    {
        foreach (var request in Command.ApprovalRequests)
        {
            var fieldRow = Db.Object.CocApprovalItems.First(item => item.Id == request.ApprovalRequestId);
            var expectedRequestToVerify = Command.ApprovalRequests.First(id => id.ApprovalRequestId == fieldRow.Id);
            fieldRow.EmployerAcknowledgedBy.Should().Be(expectedRequestToVerify.UserInfo.UserId);
            fieldRow.EmployerAcknowledgedAt.Should().NotBeNull();
        }

        if (PendingRowId != Guid.Empty)
        {
            var pendingRow = Db.Object.CocApprovalItems.Single(item => item.Id == PendingRowId);
            pendingRow.Status.Should().Be(CocApprovalItemStatus.Pending);
            pendingRow.EmployerAcknowledgedAt.Should().BeNull();
        }
    }

    public void VerifyEmployerNoAlertAcknowledged()
    {
        foreach (var request in Command.ApprovalRequests)
        {
            var approvalRequestToVerify = Db.Object.CocApprovalItems.FirstOrDefault(item => item.Id == request.ApprovalRequestId);
            var expectedRequestToVerify = Command.ApprovalRequests.Where(id => id.ApprovalRequestId == approvalRequestToVerify.Id).FirstOrDefault();
            approvalRequestToVerify.EmployerAcknowledgedAt.Should().BeNull();
            approvalRequestToVerify.EmployerAcknowledgedBy.Should().BeNull();
        }
    }

    public void Handle()
    {
        Handler.Handle(Command, CancellationToken.None).GetAwaiter().GetResult();
        Db.Object.SaveChanges();
    }
}