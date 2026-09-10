using AutoFixture.Kernel;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class CocApprovalRulesEngineTests
{
    [Test]
    public async Task Handle_WhenDeterminingApprovalState_ThenShouldMapApprovalResultCorrectly()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture();

        var result = await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Pending);
        result.ApprovalResult.Items.Should().BeEquivalentTo(fixture.CocUpdateStatuses);
    }

    [Test]
    public async Task Handle_WhenDeterminingApprovalState_ThenShouldMapApprovalRequestCorrectly()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture();

        var result = await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalRequest.LearningKey.Should().Be(fixture.ApprovalDetails.LearningKey);
        result.ApprovalRequest.ApprenticeshipId.Should().Be(fixture.ApprovalDetails.ApprenticeshipId);
        result.ApprovalRequest.LearningType.Should().Be(fixture.ApprovalDetails.LearningType);
        result.ApprovalRequest.UKPRN.Should().Be(fixture.ApprovalDetails.ProviderId.ToString());
        result.ApprovalRequest.ULN.Should().Be(fixture.ApprovalDetails.ULN);
        result.ApprovalRequest.Status.Should().Be(CocApprovalResultStatus.Pending);
        result.ApprovalRequest.Items.Should().BeEquivalentTo(fixture.ExpectedApprovalFields());
    }

    [Test]
    public async Task Handle_WhenDeterminingApprovalState_ThenShouldSetResultStatusToCompleteIfNoPending()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoApproved);
        var result = await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Complete);
    }

    [Test]
    public async Task Handle_WhenDeterminingApprovalState_ThenShouldSetResultStatusToPendingIfAnyPending()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoApproved);
        fixture.CocUpdateStatuses.First().Status = CocApprovalItemStatus.Pending;

        var result = await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Pending);
    }

    [Test]
    public async Task Handle_WhenApprovalStateIsAutoApproved_ThenShouldSendEmployerEmailNotification()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoApproved);
        await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);
        fixture.MessageSession.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(p => p.AccountId == fixture.ApprovalDetails.Apprenticeship.Cohort.EmployerAccountId &&
            p.Template == "EmployerAutoApprovedNotification" &&
            p.Tokens["provider_name"] == fixture.ApprovalDetails.Apprenticeship.Cohort.Provider.Name &&
            p.Tokens["link_to_manage_apprenticeships"].Contains(fixture.CommitmentsV2Configuration.EmployerCommitmentsBaseUrl)),
            It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenDeterminingApprovalState_ThenShouldSetResultStatusToCompleteIfAutoRejected()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoRejected);

        var result = await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Complete);
    }

    [Test]
    public async Task Handle_WhenDeterminingApprovalState_ThenShouldNotifyProviderIfAutoRejected()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoRejected);

        var result = await fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Complete);
        fixture.NotifyProviderService.Verify(x => x.NotifyProvider(It.Is<long>(p => p == fixture.ApprovalDetails.ProviderId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}

public class CocApprovalRulesEngineTestsFixture
{
    public Fixture AutoFixture { get; set; }
    public Mock<ICocApprovalStatusService> CocApprovalStatusService { get; set; }
    public CocApprovalRulesEngine Sut { get; set; }
    public CocApprovalDetails ApprovalDetails { get; set; }
    public List<CocUpdateResult> CocUpdateStatuses { get; set; }
    public List<CocApprovalFieldChange> ApprovalFieldChanges { get; set; }
    public ProviderCommitmentsDbContext DbContext { get; set; }
    public Mock<INotifyProviderService> NotifyProviderService { get; set; }
    public Mock<IMessageSession> MessageSession { get; set; }
    public CommitmentsV2Configuration CommitmentsV2Configuration { get; set; }

    public CocApprovalRulesEngineTestsFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        AutoFixture.Customizations.Add(new TypeRelay(typeof(ApprenticeshipBase), typeof(Apprenticeship)));

        CocUpdateStatuses = new List<CocUpdateResult>
        {
            new CocUpdateResult
            {
                Field = CocChangeField.TNP1,
                Status = CocApprovalItemStatus.Pending
            },
            new CocUpdateResult
            {
                Field = CocChangeField.TNP2,
                Status = CocApprovalItemStatus.Pending
            }
        };

        ApprovalFieldChanges = new List<CocApprovalFieldChange>
        {
            new CocApprovalFieldChange
            {
                ChangeType = "TNP1",
                Data = new CocData
                {
                    Old = "1000",
                    New = "1500"
                }
            },
            new CocApprovalFieldChange
            {
                ChangeType = "TNP2",
                Data = new CocData
                {
                    Old = "2000",
                    New = "1500"
                }
            }
        };

        DbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
           .Options);
        var apprenticeship = AutoFixture.Create<Apprenticeship>();
        ApprovalDetails = AutoFixture.Build<CocApprovalDetails>().With(c => c.Apprenticeship, apprenticeship).With(c => c.ApprovalFieldChanges, ApprovalFieldChanges).Create();
        CocApprovalStatusService = new Mock<ICocApprovalStatusService>();
        CocApprovalStatusService.Setup(x => x.DetermineCocUpdateStatuses(ApprovalDetails.Updates, ApprovalDetails.Apprenticeship)).Returns(CocUpdateStatuses);

        NotifyProviderService = new Mock<INotifyProviderService>();
        CommitmentsV2Configuration = AutoFixture.Create<CommitmentsV2Configuration>();
        MessageSession = new Mock<IMessageSession>();
        Sut = new CocApprovalRulesEngine(CocApprovalStatusService.Object, Mock.Of<ILogger<CocApprovalRulesEngine>>(), 
            NotifyProviderService.Object, MessageSession.Object, CommitmentsV2Configuration,
            new Mock<IEncodingService>().Object);
    }

    public CocApprovalRulesEngineTestsFixture SetCocUpdateStatuses(CocApprovalItemStatus status)
    {
        foreach (var update in CocUpdateStatuses)
        {
            update.Status = status;
        }
        return this;
    }

    public List<ApprovalFieldRequest> ExpectedApprovalFields()
    {
        return ApprovalFieldChanges.Join(
            CocUpdateStatuses,
            change => change.ChangeType,
            status => status.Field.GetEnumDescription(),
            (change, status) => new ApprovalFieldRequest
            {
                Field = change.ChangeType,
                Old = change.Data.Old,
                New = change.Data.New,
                EffectiveFromDate = change.Data.EffectiveFromDate,
                Status = status.Status,
                Reason = status.Reason
            }
        ).ToList();
    }
}