using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CocApprovals;

[TestFixture]
public class AggregationOfApprovalRequestMapperTests
{
    private Mock<IModelMapper> _modelMapper;
    private Mock<ILogger<CocApprovalRequestToCocApprovalDetailsMapper>> _logger;
    private ProviderCommitmentsDbContext _dbContext;
    private Lazy<ProviderCommitmentsDbContext> _lazyDbContext;
    private AggregationOfApprovalRequestMapper _sut;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProviderCommitmentsDbContext(options);
        _lazyDbContext = new Lazy<ProviderCommitmentsDbContext>(() => _dbContext);

        _modelMapper = new Mock<IModelMapper>();
        _logger = new Mock<ILogger<CocApprovalRequestToCocApprovalDetailsMapper>>();

        _sut = new AggregationOfApprovalRequestMapper(_lazyDbContext, _modelMapper.Object, _logger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private static CocApprovalRequest CreateChangeRequest(Guid learningKey, params CocApprovalFieldChange[] changes)
    {
        return new CocApprovalRequest
        {
            LearningKey = learningKey,
            ApprenticeshipId = 123,
            LearningType = "Standard",
            UKPRN = "12345678",
            ULN = "1234567890",
            ApprovedUri = "http://example.com/approve",
            Changes = changes.ToList()
        };
    }

    private static CocApprovalFieldChange CreateChangeItem(string field, string oldValue, string newValue)
    {
        return new CocApprovalFieldChange
        {
            ChangeType = field,
            Data = new CocData
            {
                Old = oldValue,
                New = newValue,
            }
        };
    }

    private void SetUpModelMapperReturns(CocApprovalDetails details)
    {
        _modelMapper
            .Setup(m => m.Map<CocApprovalDetails>(It.IsAny<object>()))
            .ReturnsAsync(details);
    }

    [Test]
    public async Task Map_WhenNoPreviousPendingItemsExist_ReturnsCreateNewCommand()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = CreateChangeRequest(learningKey, CreateChangeItem("TNP1", null, "10000"));
        var expectedDetails = new CocApprovalDetails { LearningKey = learningKey };
        SetUpModelMapperReturns(expectedDetails);

        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Action.Should().Be(AggregationAction.CreateNew);
        result.LearningKey.Should().Be(learningKey);
        result.CocApprovalDetails.Should().BeSameAs(expectedDetails);

        _modelMapper.Verify(m => m.Map<CocApprovalDetails>(request), Times.Once);
    }

    [Test]
    public async Task Map_WhenPreviousPendingItemsExist_AndThereAreAggregatedChanges_ReturnsSupersedePreviousCommand()
    {
        // Arrange
        var learningKey = Guid.NewGuid();

        var previousItem = new CocApprovalItem
        {
            LearningKey = learningKey,
            Status = CocApprovalItemStatus.Pending,
            Field = "TNP1",
            Old = "10000",
            New = "12000"
        };

        await _dbContext.CocApprovalItems.AddAsync(previousItem);
        await _dbContext.SaveChangesAsync();

        var newChange = CreateChangeItem("TNP1", "12000", "15000");
        var request = CreateChangeRequest(learningKey, newChange);

        var expectedDetails = new CocApprovalDetails { LearningKey = learningKey };
        SetUpModelMapperReturns(expectedDetails);

        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Action.Should().Be(AggregationAction.SupersedePrevious);
        //result.PreviousApprovalRequestId.Should().Be(previousRequestId);
        result.CocApprovalDetails.Should().BeSameAs(expectedDetails);

        // request.Changes is mutated in place by Map() before being passed to the model mapper
        request.Changes.Should().ContainSingle();
        request.Changes[0].ChangeType.Should().Be("TNP1");
        request.Changes[0].Data.Old.Should().Be("10000"); // old value comes from the previous request
        request.Changes[0].Data.New.Should().Be("15000"); // new value comes from the new request

        _modelMapper.Verify(m => m.Map<CocApprovalDetails>(request), Times.Once);
    }

    [Test]
    public async Task Map_WhenPreviousPendingRequestExists_AndNoAggregatedChangesRemain_ReturnsCancelPreviousCommand()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var previousRequestId = Guid.NewGuid();

        var previousItem = new CocApprovalItem 
        { 
            Id = Guid.NewGuid(),
            LearningKey = learningKey,
            Status = CocApprovalItemStatus.Pending,
            Field = "TNP1",
            Old = "10000",
            New = "12000",
        };

        await _dbContext.CocApprovalItems.AddAsync(previousItem);
        await _dbContext.SaveChangesAsync();

        // New change resolves back to the same old/new value as the previous request -> net no-op
        var newChange = CreateChangeItem("TNP1", "12000", "10000");
        var request = CreateChangeRequest(learningKey, newChange);

        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Action.Should().Be(AggregationAction.CancelPrevious);
        result.LearningKey.Should().Be(learningKey);
        result.CocApprovalDetails.Should().BeNull();

        _modelMapper.Verify(m => m.Map<CocApprovalDetails>(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task GetPreviousPendingApprovalItemForLearningKey_WhenPendingItemExistsForKey_ReturnsSupersedePrevious()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var existingItem = new CocApprovalItem
        {
            Id = Guid.NewGuid(),
            LearningKey = learningKey,
            Status = CocApprovalItemStatus.Pending,
            Field = "TNP1",
            New = "12000",
            Old = "10000"   
        };

        await _dbContext.CocApprovalItems.AddAsync(existingItem);
        await _dbContext.SaveChangesAsync();

        var newChange = CreateChangeItem("Firstname", "Fred", "Fred1");
        var request = CreateChangeRequest(learningKey, newChange);


        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Should().NotBeNull();
        result.LearningKey.Should().Be(learningKey);
        result.Action.Should().Be(AggregationAction.SupersedePrevious);
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldOnlyExistsInNewRequest_IsAddedAsIs()
    {
        // Arrange
        var newChange = CreateChangeItem("TNP2", null, "20000");
        var request = CreateChangeRequest(Guid.NewGuid(), newChange);
        var previousItems = new List<CocApprovalItem>();

        // Act
        var result = _sut.CreateAggregratedItems(request, previousItems);

        // Assert
        result.Should().ContainSingle();
        result[0].Should().BeSameAs(newChange);
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldExistsInBoth_AndValuesDiffer_MergesOldFromPreviousAndNewFromRequest()
    {
        // Arrange
        var newChange = CreateChangeItem("TNP1", "12000", "15000");
        var request = CreateChangeRequest(Guid.NewGuid(), newChange);

        var previousItems = new List<CocApprovalItem>() { new CocApprovalItem { Field = "TNP1", Old = "10000", New = "12000" } };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousItems);

        // Assert
        result.Should().ContainSingle();
        var merged = result[0];
        merged.ChangeType.Should().Be("TNP1");
        merged.Data.Old.Should().Be("10000");
        merged.Data.New.Should().Be("15000");
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldExistsInBoth_AndMergedOldEqualsNew_IsSkipped()
    {
        // Arrange
        // Previous request's Old value ("10000") equals the new request's New value ("10000") -> net no-op
        var newChange = CreateChangeItem("TNP1", "12000", "10000");
        var request = CreateChangeRequest(Guid.NewGuid(), newChange);

        var previousItems = new List<CocApprovalItem>() { new CocApprovalItem { Field = "TNP1", Old = "10000", New = "12000" } };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousItems);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldOnlyExistsInPreviousRequest_IsNotCarriedOver()
    {
        // Arrange
        var request = CreateChangeRequest(Guid.NewGuid()); // no changes in the new request

        var previousItem = new CocApprovalItem
        {
            Field = "TNP2",
            Old = "20000",
            New = "22000",
        };
        var previousItems = new List<CocApprovalItem> { previousItem };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousItems);

        // Assert
        result.Count.Should().Be(0);
    }

    [Test]
    public void CreateAggregratedItems_WhenMultipleFieldsInvolved_CombinesNewMergedAndCarriedOverItems()
    {
        // Arrange: one field only in new request, one field in both (merges), one field only in previous request
        var onlyNew = CreateChangeItem("LastName", null, "LastName123");
        var mergedChange = CreateChangeItem("TNP1", "12000", "15000");
        var request = CreateChangeRequest(Guid.NewGuid(), onlyNew, mergedChange);

        var previousItems = new List<CocApprovalItem>
            {
                new CocApprovalItem { Field = "TNP1", Old = "10000", New = "12000" },
                new CocApprovalItem { Field = "TNP2", Old = "20000", New = "22000" }
            };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousItems);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.ChangeType == "LastName" && c.Data.New == "LastName123");
        result.Should().Contain(c => c.ChangeType == "TNP1" && c.Data.Old == "10000" && c.Data.New == "15000");
    }
}