using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class CocApprovalRulesEngineTests
{
    [Test]
    public void Handle_WhenDeterminingApprovalState_ThenShouldMapApprovalResultCorrectly()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture();

        var result = fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Pending);
        result.ApprovalResult.Items.Should().BeEquivalentTo(fixture.CocUpdateStatuses);
    }

    [Test]
    public void Handle_WhenDeterminingApprovalState_ThenShouldMapApprovalRequestCorrectly()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture();

        var result = fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

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
    public void Handle_WhenDeterminingApprovalState_ThenShouldSetResultStatusToCompleteIfNoPending()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoApproved);

        var result = fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Complete);
    }

    [Test]
    public void Handle_WhenDeterminingApprovalState_ThenShouldSetResultStatusToPendingIfAnyPending()
    {
        var fixture = new CocApprovalRulesEngineTestsFixture().SetCocUpdateStatuses(CocApprovalItemStatus.AutoApproved);
        fixture.CocUpdateStatuses.First().Status = CocApprovalItemStatus.Pending;

        var result = fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        result.ApprovalResult.Status.Should().Be(CocApprovalResultStatus.Pending);
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

    public CocApprovalRulesEngineTestsFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

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

        ApprovalDetails = AutoFixture.Build<CocApprovalDetails>().Without(c => c.Apprenticeship).With(c => c.ApprovalFieldChanges, ApprovalFieldChanges).Create();
        CocApprovalStatusService = new Mock<ICocApprovalStatusService>();
        CocApprovalStatusService.Setup(x => x.DetermineCocUpdateStatuses(ApprovalDetails.Updates, ApprovalDetails.Apprenticeship)).Returns(CocUpdateStatuses);

        Sut = new CocApprovalRulesEngine(CocApprovalStatusService.Object, Mock.Of<ILogger<CocApprovalRulesEngine>>());
    }

    public CocApprovalRulesEngineTestsFixture SetCocUpdateStatuses(CocApprovalItemStatus status)
    {
        foreach(var update in CocUpdateStatuses)
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
                Status = status.Status,
                Reason = status.Reason
            }
        ).ToList();
    }
}