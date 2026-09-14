using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CocApprovals;

[TestFixture]
public class AggregrationOfApprovalRequestMapperTests
{
    private Mock<IModelMapper> _modelMapper;
    private Mock<ILogger<CocApprovalRequestToCocApprovalDetailsMapper>> _logger;
    private ProviderCommitmentsDbContext _dbContext;
    private Lazy<ProviderCommitmentsDbContext> _lazyDbContext;
    private AggregrationOfApprovalRequestMapper _sut;

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

        _sut = new AggregrationOfApprovalRequestMapper(_lazyDbContext, _modelMapper.Object, _logger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private static CocApprovalRequest CreateRequest(Guid learningKey, params CocApprovalFieldChange[] changes)
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

    private static CocApprovalFieldChange CreateChange(string field, string oldValue, string newValue, DateTime? effectiveFrom = null)
    {
        return new CocApprovalFieldChange
        {
            ChangeType = field,
            Data = new CocData
            {
                Old = oldValue,
                New = newValue,
                EffectiveFromDate = effectiveFrom
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
    public async Task Map_WhenNoPreviousPendingRequestExists_ReturnsCreateNewCommand()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = CreateRequest(learningKey, CreateChange("TNP1", null, "10000"));
        var expectedDetails = new CocApprovalDetails { LearningKey = learningKey };
        SetUpModelMapperReturns(expectedDetails);

        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Action.Should().Be(AggregrationAction.CreateNew);
        result.PreviousApprovalRequestId.Should().BeNull();
        result.CocApprovalDetails.Should().BeSameAs(expectedDetails);

        _modelMapper.Verify(m => m.Map<CocApprovalDetails>(request), Times.Once);
    }

    [Test]
    public async Task Map_WhenPreviousPendingRequestExists_AndThereAreAggregatedChanges_ReturnsSupersedePreviousCommand()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var previousRequestId = Guid.NewGuid();

        var previousRequest = new ApprovalRequest
        {
            Id = previousRequestId,
            LearningKey = learningKey,
            Status = CocApprovalResultStatus.Pending,
            Items = new List<ApprovalFieldRequest>
            {
                new ApprovalFieldRequest
                {
                    Id = Guid.NewGuid(),
                    Field = "TNP1",
                    Old = "10000",
                    New = "12000",
                    EffectiveFromDate = new DateTime(2025, 1, 1)
                }
            }
        };

        await _dbContext.ApprovalRequests.AddAsync(previousRequest);
        await _dbContext.SaveChangesAsync();

        var newChange = CreateChange("TNP1", "12000", "15000", new DateTime(2025, 6, 1));
        var request = CreateRequest(learningKey, newChange);

        var expectedDetails = new CocApprovalDetails { LearningKey = learningKey };
        SetUpModelMapperReturns(expectedDetails);

        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Action.Should().Be(AggregrationAction.SupersedePrevious);
        result.PreviousApprovalRequestId.Should().Be(previousRequestId);
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

        var previousRequest = new ApprovalRequest
        {
            Id = previousRequestId,
            LearningKey = learningKey,
            Status = CocApprovalResultStatus.Pending,
            Items = new List<ApprovalFieldRequest>
            {
                new ApprovalFieldRequest
                {
                    Id = Guid.NewGuid(),
                    Field = "TNP1",
                    Old = "10000",
                    New = "12000",
                    EffectiveFromDate = new DateTime(2025, 1, 1)
                }
            }
        };

        await _dbContext.ApprovalRequests.AddAsync(previousRequest);
        await _dbContext.SaveChangesAsync();

        // New change resolves back to the same old/new value as the previous request -> net no-op
        var newChange = CreateChange("TNP1", "12000", "10000", new DateTime(2025, 6, 1));
        var request = CreateRequest(learningKey, newChange);

        // Act
        var result = await _sut.Map(request);

        // Assert
        result.Action.Should().Be(AggregrationAction.CancelPrevious);
        result.PreviousApprovalRequestId.Should().Be(previousRequestId);
        result.CocApprovalDetails.Should().BeNull();

        _modelMapper.Verify(m => m.Map<CocApprovalDetails>(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task GetPreviousPendingApprovalRequestForLearningKey_WhenPendingRequestExistsForKey_ReturnsIt()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var approvalRequest = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            LearningKey = learningKey,
            Status = CocApprovalResultStatus.Pending,
            Items = new List<ApprovalFieldRequest>()
        };

        await _dbContext.ApprovalRequests.AddAsync(approvalRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousPendingApprovalRequestForLearningKey(learningKey);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(approvalRequest.Id);
    }

    [Test]
    public async Task GetPreviousPendingApprovalRequestForLearningKey_WhenRequestForKeyIsNotPending_ReturnsNull()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var approvalRequest = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            LearningKey = learningKey,
            Status = CocApprovalResultStatus.Complete,
            Items = new List<ApprovalFieldRequest>()
        };

        await _dbContext.ApprovalRequests.AddAsync(approvalRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousPendingApprovalRequestForLearningKey(learningKey);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetPreviousPendingApprovalRequestForLearningKey_WhenNoRequestMatchesKey_ReturnsNull()
    {
        // Arrange
        var approvalRequest = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            Status = CocApprovalResultStatus.Pending,
            Items = new List<ApprovalFieldRequest>()
        };

        await _dbContext.ApprovalRequests.AddAsync(approvalRequest);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousPendingApprovalRequestForLearningKey(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldOnlyExistsInNewRequest_IsAddedAsIs()
    {
        // Arrange
        var newChange = CreateChange("TNP2", null, "20000", new DateTime(2025, 3, 1));
        var request = CreateRequest(Guid.NewGuid(), newChange);
        var previousRequest = new ApprovalRequest { Items = new List<ApprovalFieldRequest>() };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousRequest);

        // Assert
        result.Should().ContainSingle();
        result[0].Should().BeSameAs(newChange);
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldExistsInBoth_AndValuesDiffer_MergesOldFromPreviousAndNewFromRequest()
    {
        // Arrange
        var newChange = CreateChange("TNP1", "12000", "15000", new DateTime(2025, 6, 1));
        var request = CreateRequest(Guid.NewGuid(), newChange);

        var previousRequest = new ApprovalRequest
        {
            Items = new List<ApprovalFieldRequest>
            {
                new ApprovalFieldRequest { Field = "TNP1", Old = "10000", New = "12000" }
            }
        };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousRequest);

        // Assert
        result.Should().ContainSingle();
        var merged = result[0];
        merged.ChangeType.Should().Be("TNP1");
        merged.Data.Old.Should().Be("10000");
        merged.Data.New.Should().Be("15000");
        merged.Data.EffectiveFromDate.Should().Be(new DateTime(2025, 6, 1));
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldExistsInBoth_AndMergedOldEqualsNew_IsSkipped()
    {
        // Arrange
        // Previous request's Old value ("10000") equals the new request's New value ("10000") -> net no-op
        var newChange = CreateChange("TNP1", "12000", "10000", new DateTime(2025, 6, 1));
        var request = CreateRequest(Guid.NewGuid(), newChange);

        var previousRequest = new ApprovalRequest
        {
            Items = new List<ApprovalFieldRequest>
            {
                new ApprovalFieldRequest { Field = "TNP1", Old = "10000", New = "12000" }
            }
        };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void CreateAggregratedItems_WhenFieldOnlyExistsInPreviousRequest_IsCarriedOver()
    {
        // Arrange
        var request = CreateRequest(Guid.NewGuid()); // no changes in the new request

        var previousItem = new ApprovalFieldRequest
        {
            Field = "TNP2",
            Old = "20000",
            New = "22000",
            EffectiveFromDate = new DateTime(2025, 2, 1)
        };
        var previousRequest = new ApprovalRequest
        {
            Items = new List<ApprovalFieldRequest> { previousItem }
        };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousRequest);

        // Assert
        result.Should().ContainSingle();
        var carried = result[0];
        carried.ChangeType.Should().Be("TNP2");
        carried.Data.Old.Should().Be("20000");
        carried.Data.New.Should().Be("22000");
        carried.Data.EffectiveFromDate.Should().Be(new DateTime(2025, 2, 1));
    }

    [Test]
    public void CreateAggregratedItems_WhenMultipleFieldsInvolved_CombinesNewMergedAndCarriedOverItems()
    {
        // Arrange: one field only in new request, one field in both (merges), one field only in previous request
        var onlyNew = CreateChange("LastName", null, "LastName123");
        var mergedChange = CreateChange("TNP1", "12000", "15000");
        var request = CreateRequest(Guid.NewGuid(), onlyNew, mergedChange);

        var previousRequest = new ApprovalRequest
        {
            Items = new List<ApprovalFieldRequest>
            {
                new ApprovalFieldRequest { Field = "TNP1", Old = "10000", New = "12000" },
                new ApprovalFieldRequest { Field = "TNP2", Old = "20000", New = "22000" }
            }
        };

        // Act
        var result = _sut.CreateAggregratedItems(request, previousRequest);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.ChangeType == "LastName" && c.Data.New == "LastName123");
        result.Should().Contain(c => c.ChangeType == "TNP1" && c.Data.Old == "10000" && c.Data.New == "15000");
        result.Should().Contain(c => c.ChangeType == "TNP2" && c.Data.Old == "20000" && c.Data.New == "22000");
    }
}