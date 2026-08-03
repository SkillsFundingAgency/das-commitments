using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;
[TestFixture]
public class ProcessApprenticeshipApprovalCommandHandlerTests
{
    private Fixture _autoFixture;
    private ProcessApprenticeshipApprovalCommandHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _autoFixture = new Fixture();
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

        public ProcessApprenticeshipApprovalCommandHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            MessageSession = new Mock<IMessageSession>();

            Handler = new ProcessApprenticeshipApprovalCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                MessageSession.Object);

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
                .With(x => x.Status, CocApprovalResultStatus.Pending).Create();
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
        }

        public async Task<ProcessApprenticeshipApprovalCommandHandlerTestsFixture> SeedData()
        {
            if (ApprovalRequest != null)
            {
                Db.ApprovalRequests.Add(ApprovalRequest);
                await Db.SaveChangesAsync();
            }
            return this;
        }
    }
}
