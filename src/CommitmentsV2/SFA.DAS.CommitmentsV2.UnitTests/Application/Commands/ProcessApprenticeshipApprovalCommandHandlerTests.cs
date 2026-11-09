using AutoFixture.Kernel;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
public class ProcessApprenticeshipApprovalCommandHandlerTests
{
    private ProcessApprenticeshipApprovalCommandHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ProcessApprenticeshipApprovalCommandHandlerTestsFixture();
    }

    [Test]
    public async Task When_HandlingCommand_And_ApprovalRequest_NotFound_Throw_Exception()
    {
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"No pending changes were found for apprenticeship {_fixture.Command.ApprenticeshipId}");
    }

    [Test]
    public async Task When_HandlingCommand_And_ApprovalRequest_Found_But_Wrong_Apprenticeship_Throw_Exception()
    {
        foreach (var item in _fixture.PendingItems)
        {
            item.ApprenticeshipId = _fixture.Command.ApprenticeshipId + 1;
        }
        await _fixture.SeedData();
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"No pending changes were found for apprenticeship {_fixture.Command.ApprenticeshipId}");
    }

    [Test]
    public async Task When_HandlingCommand_And_ApprovalRequest_Found_But_Status_Not_Pending_Throw_Exception()
    {
        foreach (var item in _fixture.PendingItems)
        {
            item.Status = CocApprovalItemStatus.EmployerApproved;
        }
        await _fixture.SeedData();
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"No pending changes were found for apprenticeship {_fixture.Command.ApprenticeshipId}");
    }

    [Test]
    public async Task When_HandlingCommand_Should_Send_Command_ToChangeHistory()
    {
        await _fixture.SeedData();
        await _fixture.Handle();

        _fixture.MessageSession.Verify(y => y.Send(It.Is<StoreLearningHistoryCommand>(x => x.ApprenticeshipId == _fixture.Command.ApprenticeshipId &&
            x.Source == LearningSourceType.ApprovalAPI &&
            x.ChangeType == (_fixture.Command.ApplyChanges ? LearningChangeType.EmployerApproved : LearningChangeType.EmployerRejected) &&
            x.Description == "Total price change from £1,100 to £2,200"
            ), It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_With_PlannedEndDate_Should_Send_Command_ToChangeHistory()
    {
        _fixture.PendingItems.Add(_fixture.CreatePlannedEndDateItem());
        await _fixture.SeedData();
        await _fixture.Handle();

        _fixture.MessageSession.Verify(y => y.Send(It.Is<StoreLearningHistoryCommand>(x =>
            x.ApprenticeshipId == _fixture.Command.ApprenticeshipId &&
            x.ChangeType == LearningChangeType.EmployerApproved &&
            x.Description == "Planned end date change from 2026-06-01 to 2026-12-01"
        ), It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_Should_SaveRequestAsApproved()
    {
        _fixture.Command.ApplyChanges = true;
        await _fixture.SeedData();
        await _fixture.Handle();

        var items = await _fixture.Db.CocApprovalItems.Where(x => x.ApprenticeshipId == _fixture.Command.ApprenticeshipId).ToListAsync();

        items.Should().OnlyContain(item => item.Status == CocApprovalItemStatus.EmployerApproved);

        _fixture.NotifyProviderService.Verify(x => x.NotifyProvider(
           _fixture.Apprenticeship.Cohort.ProviderId,
           It.Is<string>(t => t == "encoded-apprenticeship-id"),
           It.Is<string>(t => t == "ProviderLearningChangeApprovedNotification"),
           It.Is<string>(t => t == _fixture.AccountLegalEntity.Name)), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_Should_SaveRequestAsRejected()
    {
        _fixture.Command.ApplyChanges = false;
        await _fixture.SeedData();
        await _fixture.Handle();

        var items = await _fixture.Db.CocApprovalItems.Where(x => x.ApprenticeshipId == _fixture.Command.ApprenticeshipId).ToListAsync();

        items.Should().OnlyContain(item => item.Status == CocApprovalItemStatus.EmployerRejected);

        _fixture.NotifyProviderService.Verify(x => x.NotifyProvider(
            _fixture.Apprenticeship.Cohort.ProviderId,
            It.Is<string>(t => t == "encoded-apprenticeship-id"),
            It.Is<string>(t => t == "ProviderLearningChangeRejectedNotification"),
            It.Is<string>(t => t == _fixture.AccountLegalEntity.Name)), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_Should_Publish_LearningChangeApprovedEvent()
    {
        _fixture.Command.ApplyChanges = true;
        await _fixture.SeedData();
        await _fixture.Handle();

        _fixture.MessageSession.Verify(y => y.Publish(It.Is<LearningChangeApprovedEvent>(x =>
            x.ApprenticeshipId == _fixture.Command.ApprenticeshipId &&
            x.LearningKey == _fixture.LearningKey &&
            x.Changes.Count(change =>
                change.ChangeType == nameof(CocChangeField.Prices) &&
                change.Old == _fixture.OldPricesJson &&
                change.New == _fixture.NewPricesJson) == 1), It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_Should_Publish_LearningChangeRejectedEvent()
    {
        _fixture.Command.ApplyChanges = false;
        await _fixture.SeedData();
        await _fixture.Handle();

        _fixture.MessageSession.Verify(y => y.Publish(It.Is<LearningChangeRejectedEvent>(x =>
            x.ApprenticeshipId == _fixture.Command.ApprenticeshipId &&
            x.LearningKey == _fixture.LearningKey &&
            x.Changes.Count(change =>
                change.ChangeType == nameof(CocChangeField.Prices) &&
                change.Old == _fixture.OldPricesJson &&
                change.New == _fixture.NewPricesJson) == 1), It.IsAny<PublishOptions>()), Times.Once);
    }

    [TestCase(PaymentStatus.Withdrawn, true)]
    [TestCase(PaymentStatus.Withdrawn, false)]
    [TestCase(PaymentStatus.Completed, true)]
    [TestCase(PaymentStatus.Completed, false)]
    public async Task When_HandlingCommand_For_Withdrawn_Or_Completed_Apprenticeship_Should_Process_Decision_And_Keep_Status(PaymentStatus paymentStatus, bool applyChanges)
    {
        var stopDate = DateTime.UtcNow.AddMonths(-1).Date;
        _fixture.Apprenticeship.PaymentStatus = paymentStatus;
        _fixture.Apprenticeship.StopDate = stopDate;
        _fixture.Apprenticeship.WithdrawnReasonCode = paymentStatus == PaymentStatus.Withdrawn ? 1 : null;
        _fixture.Command.ApplyChanges = applyChanges;
        await _fixture.SeedData();
        await _fixture.Handle();

        var expectedStatus = applyChanges ? CocApprovalItemStatus.EmployerApproved : CocApprovalItemStatus.EmployerRejected;
        var items = await _fixture.Db.CocApprovalItems.Where(x => x.ApprenticeshipId == _fixture.Command.ApprenticeshipId).ToListAsync();
        items.Should().OnlyContain(item => item.Status == expectedStatus);

        var apprenticeship = await _fixture.Db.Apprenticeships.FindAsync(_fixture.Command.ApprenticeshipId);
        apprenticeship.PaymentStatus.Should().Be(paymentStatus);
        apprenticeship.StopDate.Should().Be(stopDate);
        apprenticeship.WithdrawnReasonCode.Should().Be(_fixture.Apprenticeship.WithdrawnReasonCode);
    }

    public class ProcessApprenticeshipApprovalCommandHandlerTestsFixture
    {
        public ProcessApprenticeshipApprovalCommandHandler Handler;
        public ProcessApprenticeshipApprovalCommand Command;
        public ProviderCommitmentsDbContext Db { get; set; }
        public List<CocApprovalItem> PendingItems;
        public Guid LearningKey;
        public Mock<IMessageSession> MessageSession;
        public AccountLegalEntity AccountLegalEntity;
        public Mock<INotifyProviderService> NotifyProviderService;
        public Mock<IEncodingService> EncodingService;
        public string OldPricesJson;
        public string NewPricesJson;

        public Apprenticeship Apprenticeship;

        public ProcessApprenticeshipApprovalCommandHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            autoFixture.Customizations.Add(
                new TypeRelay(
                    typeof(SFA.DAS.CommitmentsV2.Models.ApprenticeshipBase),
                    typeof(Apprenticeship)));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            MessageSession = new Mock<IMessageSession>();
            NotifyProviderService = new Mock<INotifyProviderService>();
            EncodingService = new Mock<IEncodingService>();

            EncodingService.Setup(x => x.Encode(It.IsAny<long>(), It.IsAny<EncodingType>()))
                .Returns("encoded-apprenticeship-id");

            NotifyProviderService.Setup(x => x.NotifyProvider(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            Handler = new ProcessApprenticeshipApprovalCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                MessageSession.Object,
                NotifyProviderService.Object, EncodingService.Object);

            Command = autoFixture.Create<ProcessApprenticeshipApprovalCommand>();
            LearningKey = Guid.NewGuid();
            OldPricesJson = Schedule(1100m, 1000m, 100m);
            NewPricesJson = Schedule(2200m, 2000m, 200m);
            PendingItems =
            [
                CreateItem(nameof(CocChangeField.Prices), OldPricesJson, NewPricesJson)
            ];

            var AccountLegalEntityId = autoFixture.Create<long>();

            var account = new Account(1, autoFixture.Create<string>(), autoFixture.Create<string>(), autoFixture.Create<string>(), DateTime.UtcNow);

            AccountLegalEntity = new AccountLegalEntity(account,
                AccountLegalEntityId,
                0,
                "",
                publicHashedId: autoFixture.Create<string>(),
                autoFixture.Create<string>(),
                OrganisationType.PublicBodies,
                "",
                DateTime.UtcNow);

            var Cohort = new Cohort
            {
                Id = autoFixture.CreateMany<long>().Last(),
                AccountLegalEntity = AccountLegalEntity,
                EmployerAccountId = autoFixture.Create<long>(),
                ProviderId = 12345,
                Reference = autoFixture.Create<string>()
            };

            Apprenticeship = autoFixture.Build<Apprenticeship>()
                .With(x => x.Id, Command.ApprenticeshipId)
                .With(x => x.Cohort, Cohort)
                .With(x => x.CommitmentId, Cohort.Id)
                .Without(x => x.CocApprovalItems)
                .Create();
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
        }

        public async Task<ProcessApprenticeshipApprovalCommandHandlerTestsFixture> SeedData()
        {
            Db.Apprenticeships.Add(Apprenticeship);
            Db.CocApprovalItems.AddRange(PendingItems);
            await Db.SaveChangesAsync();
            return this;
        }

        public CocApprovalItem CreatePlannedEndDateItem()
        {
            return CreateItem(nameof(CocChangeField.PlannedEndDate), "2026-06-01", "2026-12-01");
        }

        public static string Schedule(decimal cost, decimal trainingPrice = 0m, decimal assessmentPrice = 0m)
        {
            if (trainingPrice == 0m && assessmentPrice == 0m)
            {
                trainingPrice = cost;
            }

            return
                $$"""[{"effectiveFrom":"2024-08-01","cost":{{cost.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"trainingPrice":{{trainingPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"assessmentPrice":{{assessmentPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}]""";
        }

        private CocApprovalItem CreateItem(string field, string oldValue, string newValue)
        {
            return new CocApprovalItem
            {
                Id = Guid.NewGuid(),
                ApprenticeshipId = Command.ApprenticeshipId,
                LearningKey = LearningKey,
                Status = CocApprovalItemStatus.Pending,
                Field = field,
                Old = oldValue,
                New = newValue,
                LearningType = CocLearningType.Apprenticeship,
                UKPRN = "10000000",
                ULN = "1234567890"
            };
        }
    }
}
