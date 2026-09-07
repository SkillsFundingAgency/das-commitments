using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
public class LearningChangeHistoryDescriptionBuilderTests
{
    [Test]
    public void Build_WhenItemsAreNull_ReturnsEmptyString()
    {
        LearningChangeHistoryDescriptionBuilder.Build(null).Should().BeEmpty();
    }

    [Test]
    public void Build_WhenNoAutoRejectedItems_ReturnsEmptyString()
    {
        var items = new List<ApprovalFieldRequest>
        {
            new() { Field = "TNP1", Old = "8000", New = "7000", Status = CocApprovalItemStatus.AutoApproved },
            new() { Field = "TNP2", Old = "0", New = "0", Status = CocApprovalItemStatus.Pending }
        };

        LearningChangeHistoryDescriptionBuilder.Build(items).Should().BeEmpty();
    }

    [Test]
    public void Build_WhenTnpFieldsAreAutoRejected_ReturnsTotalPriceDescription()
    {
        var items = new List<ApprovalFieldRequest>
        {
            new() { Field = "TNP1", Old = "5000", New = "0", Status = CocApprovalItemStatus.AutoRejected },
            new() { Field = "TNP2", Old = "3000", New = "0", Status = CocApprovalItemStatus.AutoRejected }
        };

        var expected = $"Total price change from {8000m.ToGdsCostFormat()} to {0m.ToGdsCostFormat()}";

        LearningChangeHistoryDescriptionBuilder.Build(items).Should().Be(expected);
    }

    [Test]
    public void Build_WhenOnlyAutoRejectedTnpFieldsAreIncludedInTotal()
    {
        var items = new List<ApprovalFieldRequest>
        {
            new() { Field = "TNP1", Old = "8000", New = "0", Status = CocApprovalItemStatus.AutoRejected },
            new() { Field = "TNP2", Old = "2000", New = "2000", Status = CocApprovalItemStatus.AutoApproved }
        };

        var expected = $"Total price change from {8000m.ToGdsCostFormat()} to {0m.ToGdsCostFormat()}";

        LearningChangeHistoryDescriptionBuilder.Build(items).Should().Be(expected);
    }

    [Test]
    public void Build_WhenNonPriceFieldIsAutoRejected_ReturnsFieldDescription()
    {
        var items = new List<ApprovalFieldRequest>
        {
            new() { Field = "DateOfBirth", Old = "2000-01-01", New = "2001-01-01", Status = CocApprovalItemStatus.AutoRejected }
        };

        LearningChangeHistoryDescriptionBuilder.Build(items)
            .Should().Be("Date of birth change from 2000-01-01 to 2001-01-01");
    }

    [Test]
    public void Build_WhenPriceAndOtherFieldsAreAutoRejected_JoinsDescriptions()
    {
        var items = new List<ApprovalFieldRequest>
        {
            new() { Field = "TNP1", Old = "8000", New = "0", Status = CocApprovalItemStatus.AutoRejected },
            new() { Field = "DateOfBirth", Old = "2000-01-01", New = "2001-01-01", Status = CocApprovalItemStatus.AutoRejected }
        };

        var expected =
            $"Total price change from {8000m.ToGdsCostFormat()} to {0m.ToGdsCostFormat()}. Date of birth change from 2000-01-01 to 2001-01-01";

        LearningChangeHistoryDescriptionBuilder.Build(items).Should().Be(expected);
    }
}
