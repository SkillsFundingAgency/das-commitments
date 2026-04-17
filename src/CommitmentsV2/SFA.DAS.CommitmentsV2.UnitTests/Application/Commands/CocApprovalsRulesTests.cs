using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class CocApprovalRulesTests
{
    [Test]
    public void Handle_WhenHandlingCommandAndNoExistingApprovalRequest_ThenShouldThrowNullArgumentException()
    {
        var fixture = new CocApprovalRulesTestsFixture();

        var result = fixture.Sut.DetermineApprovalState(fixture.ApprovalDetails);

        result.Should().NotBeNull();
        //result.Status.Should().Be(CocApprovalResultStatus.Pending);
        //result.Items.Should().BeEquivalentTo(fixture.CocUpdateStatuses);
    }
}

public class CocApprovalRulesTestsFixture
{
    public Fixture AutoFixture { get; set; }
    public Mock<ICocApprovalStatusService> CocApprovalStatusService { get; set; }
    public CocApprovalRules Sut { get; set; }
    public CocApprovalDetails ApprovalDetails { get; set; }
    public List<CocUpdateResult> CocUpdateStatuses { get; set; }
    public List<CocApprovalFieldChange> ApprovalFieldChanges { get; set; }

    public CocApprovalRulesTestsFixture()
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

        Sut = new CocApprovalRules(CocApprovalStatusService.Object, Mock.Of<ILogger<CocApprovalRules>>());
    }
}
