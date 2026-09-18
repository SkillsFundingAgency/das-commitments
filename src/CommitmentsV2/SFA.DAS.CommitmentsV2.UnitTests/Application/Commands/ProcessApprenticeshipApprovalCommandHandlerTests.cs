using AutoFixture.Kernel;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
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
        _fixture.ApprovalRequest = null;
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<Exception>().WithMessage($"Approval request {_fixture.Command.ApprovalRequestId} not found");
    }

    [Test]
    public async Task When_HandlingCommand_And_ApprovalRequest_Found_But_Wrong_Apprenticeship_Throw_Exception()
    {
        _fixture.ApprovalRequest.ApprenticeshipId = _fixture.Command.ApprenticeshipId + 1;
        await _fixture.SeedData();
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<Exception>().WithMessage($"Approval request {_fixture.Command.ApprovalRequestId} not found for apprenticeship {_fixture.Command.ApprenticeshipId}");
    }

    [Test]
    public async Task When_HandlingCommand_And_ApprovalRequest_Found_But_Status_Not_Pending_Throw_Exception()
    {
        var items = new List<ApprovalFieldRequest>();
        _fixture.ApprovalRequest.Status = CocApprovalResultStatus.Cancelled;
        await _fixture.SeedData();
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<Exception>().WithMessage($"Approval request {_fixture.Command.ApprovalRequestId} is no longer pending. It's status is {_fixture.ApprovalRequest.Status}");
    }

    [Test]
    public async Task When_HandlingCommand_And_ApprovalRequest_TNPValues_Exceed_Upper_Limit_Throw_DomainException()
    {
        var items = new List<ApprovalFieldRequest>();
        await _fixture.SeedData();
        _fixture.ApprovalRequest.Items.First(x => x.Field == "TNP1").New = "100001";
        var act = async () => await _fixture.Handle();
        await act.Should().ThrowAsync<DomainException>();
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
    public async Task When_HandlingCommand_Should_SaveRequestAsApproved()
    {
        _fixture.Command.ApplyChanges = true;
        await _fixture.SeedData();
        await _fixture.Handle();

        var request = await _fixture.Db.ApprovalRequests.FirstOrDefaultAsync(x => x.Id == _fixture.Command.ApprovalRequestId);

        request.Status.Should().Be(CocApprovalResultStatus.Complete);
        request.Items.First().Status.Should().Be(CocApprovalItemStatus.EmployerApproved);
        request.Items.First().ApproverId.Should().Be(_fixture.Command.UserInfo.UserId);
        request.Items.Last().Status.Should().Be(CocApprovalItemStatus.EmployerApproved);
        request.Items.Last().ApproverId.Should().Be(_fixture.Command.UserInfo.UserId);
    }

    [Test]
    public async Task When_HandlingCommand_Should_SaveRequestAsRejected()
    {
        _fixture.Command.ApplyChanges = false;
        await _fixture.SeedData();
        await _fixture.Handle();

        var request = await _fixture.Db.ApprovalRequests.FirstOrDefaultAsync(x => x.Id == _fixture.Command.ApprovalRequestId);

        request.Status.Should().Be(CocApprovalResultStatus.Complete);
        request.Items.First().Status.Should().Be(CocApprovalItemStatus.EmployerRejected);
        request.Items.First().ApproverId.Should().Be(_fixture.Command.UserInfo.UserId);
        request.Items.Last().Status.Should().Be(CocApprovalItemStatus.EmployerRejected);
        request.Items.Last().ApproverId.Should().Be(_fixture.Command.UserInfo.UserId);

        _fixture.NotifyProviderService.Verify(x => x.NotifyProvider(
            _fixture.ApprovalRequest.Apprenticeship.Cohort.ProviderId,
            It.Is<string>(t => t == "encoded-apprenticeship-id"),
            It.Is<string>(t => t == "ProviderLearningChangeRejectedNotification"),
            It.Is<string>(t=> t== _fixture.AccountLegalEntity.Name)), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_Should_Publish_LearningChangeApprovedEvent()
    {
        _fixture.Command.ApplyChanges = true;
        await _fixture.SeedData();
        await _fixture.Handle();

        _fixture.MessageSession.Verify(y => y.Publish(It.Is<LearningChangeApprovedEvent>(x => x.ApprenticeshipId == _fixture.Command.ApprenticeshipId &&
            x.LearningKey == _fixture.ApprovalRequest.LearningKey &&
            x.Changes["TrainingPrice"].Old == "1000" &&
            x.Changes["TrainingPrice"].New == "2000" &&
            x.Changes["AssessmentPrice"].Old == "100" &&
            x.Changes["AssessmentPrice"].New == "200"), It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task When_HandlingCommand_Should_Publish_LearningChangeRejectedEvent()
    {
        _fixture.Command.ApplyChanges = false;
        await _fixture.SeedData();
        await _fixture.Handle();

        _fixture.MessageSession.Verify(y => y.Publish(It.Is<LearningChangeRejectedEvent>(x => x.ApprenticeshipId == _fixture.Command.ApprenticeshipId &&
            x.LearningKey == _fixture.ApprovalRequest.LearningKey &&
            x.Changes["TrainingPrice"].Old == "1000" &&
            x.Changes["TrainingPrice"].New == "2000" &&
            x.Changes["AssessmentPrice"].Old == "100" &&
            x.Changes["AssessmentPrice"].New == "200"), It.IsAny<PublishOptions>()), Times.Once);
    }

    public class ProcessApprenticeshipApprovalCommandHandlerTestsFixture
    {
        public ProcessApprenticeshipApprovalCommandHandler Handler;
        public ProcessApprenticeshipApprovalCommand Command;
        public ProviderCommitmentsDbContext Db { get; set; }
        public ApprovalRequest ApprovalRequest;
        public List<ApprovalFieldRequest> Items;
        public Mock<IMessageSession> MessageSession;
        public AccountLegalEntity AccountLegalEntity;
        public Mock<INotifyProviderService> NotifyProviderService;
        public Mock<IEncodingService> EncodingService; 


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
            Items =
            [
                autoFixture.Build<ApprovalFieldRequest>()
                    .With(x => x.Id, Guid.NewGuid())
                    .With(x => x.Field, "TNP1")
                    .With(x => x.Old, "1000")
                    .With(x => x.New, "2000")
                    .Without(x => x.ApprovalRequestId)
                    .Without(x => x.ApprovalRequest)
                    .Create(),
                autoFixture.Build<ApprovalFieldRequest>()
                    .With(x => x.Id, Guid.NewGuid())
                    .With(x => x.Field, "TNP2")
                    .With(x => x.Old, "100")
                    .With(x => x.New, "200")
                    .Without(x => x.ApprovalRequestId)
                    .Without(x => x.ApprovalRequest)
                    .Create(),
            ];

            ApprovalRequest = autoFixture.Build<ApprovalRequest>()
                .With(x => x.Items, Items)
                .With(x => x.Id, Command.ApprovalRequestId)
                .With(x => x.ApprenticeshipId, Command.ApprenticeshipId)
                .With(x => x.Status, CocApprovalResultStatus.Pending)
                .Without(x => x.Apprenticeship)
                .Create();

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
                .Create();
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
        }

        public async Task<ProcessApprenticeshipApprovalCommandHandlerTestsFixture> SeedData()
        {
            if (ApprovalRequest != null)
            {
                Db.Apprenticeships.Add(Apprenticeship);
                Db.ApprovalRequests.Add(ApprovalRequest);
                await Db.SaveChangesAsync();
            }
            return this;
        }
    }
}