using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class ApprovalsControllerTests
{
    private Fixture _fixture;
    private Mock<IMediator> _mediator;
    private Mock<IModelMapper> _mapper;
    private ApprovalsController _controller;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mediator = new Mock<IMediator>();
        _mapper = new Mock<IModelMapper>();
        _controller = new ApprovalsController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<ApprovalsController>>());
    }

    [Test]
    public async Task CocApprovals_Processes_A_New_Request_Then_ReturnsResponse()
    {
        var request = CreateCocApprovalRequest(true);
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>().With(x => x.CocApprovalDetails, cocApprovalDetails).With(x => x.Action, AggregationAction.CreateNew).Create();
        var commandResult = CreateCocResult();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);
        commandResult.Items.Where(x => x.Status == CocApprovalItemStatus.Pending).ToList().ForEach(i => i.Status = CocApprovalItemStatus.AutoApproved);

        var result = await _controller.PutApprovals(request.LearningKey, request);

        result.Should().NotBeNull();
        var jsonResult = result.Should().BeOfType<CreatedResult>().Subject;
        jsonResult.StatusCode.Should().Be(201);
        var response = jsonResult.Value.Should().BeOfType<PutApprovalsResponse>().Subject;
        response.Prices.Should().BeNull();
        response.Changes.Should().BeEquivalentTo(commandResult.Items.Select(x => new
        {
            ChangeType = x.Field.GetEnumDescription(),
            ApprovalStatus = x.Status.GetEnumDescription(),
            x.Reason,
            Data = request.Changes.Single(c => c.ChangeType == x.Field.GetEnumDescription()).Data
        }).ToList());
    }

    [Test]
    public async Task PostApprovals_Returns_BadRequest_When_LearningKey_Not_Matching()
    {
        var request = _fixture.Create<CocApprovalRequest>();

        var result = await _controller.PutApprovals(Guid.NewGuid(), request);

        result.Should().NotBeNull();
        result.Should().BeOfType<BadRequestObjectResult>();
        var jsonResult = result as BadRequestObjectResult;
        jsonResult.StatusCode.Should().Be(400);
    }

    [Test]
    public async Task CocApprovals_Processes_A_Request_And_Pending_Request_Exists_Then_ReturnsResponse()
    {
        var request = CreateCocApprovalRequest(true);
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .With(x => x.CocApprovalDetails, cocApprovalDetails)
            .With(x => x.Action, AggregationAction.SupersedePrevious).Create();
        var commandResult = CreateCocResult();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);
        commandResult.Items.ForEach(i => i.Status = CocApprovalItemStatus.Pending);

        var result = await _controller.PutApprovals(request.LearningKey, request);

        result.Should().NotBeNull();
        var jsonResult = result.Should().BeOfType<CreatedResult>().Subject;
        jsonResult.StatusCode.Should().Be(201);
        var response = jsonResult.Value.Should().BeOfType<PutApprovalsResponse>().Subject;
        response.Prices.Should().BeNull();
        response.Changes.Should().BeEquivalentTo(commandResult.Items.Select(x => new
        {
            ChangeType = x.Field.GetEnumDescription(),
            ApprovalStatus = "EmployerApprovalRequested",
            x.Reason,
            Data = request.Changes.Single(c => c.ChangeType == x.Field.GetEnumDescription()).Data
        }).ToList());
    }

    [Test]
    public async Task CocApprovals_ReturnsAutoApproved_WhenTheStoredPriceStatusIsReversal()
    {
        var request = CreateCocApprovalRequest(true);
        request.Prices =
        [
            new PriceRecord { TrainingPrice = 1200m, AssessmentPrice = 900m, EffectiveFrom = new DateTime(2026, 7, 8) }
        ];
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .With(x => x.CocApprovalDetails, cocApprovalDetails)
            .With(x => x.Action, AggregationAction.CreateNew)
            .Create();
        var commandResult = new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Complete,
            Items =
            [
                new CocUpdateResult
                {
                    Field = CocChangeField.Prices,
                    Status = CocApprovalItemStatus.Reversal
                }
            ]
        };

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);

        var result = await _controller.PutApprovals(request.LearningKey, request);

        var jsonResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = jsonResult.Value.Should().BeOfType<PutApprovalsResponse>().Subject;
        response.Prices.Should().BeSameAs(request.Prices);
        response.Changes.Should().BeEquivalentTo(new[]
        {
            new { ChangeType = "Prices", ApprovalStatus = "AutoApproved", Reason = (string)null, Data = (CocData)null },
            new
            {
                ChangeType = "Firstname",
                ApprovalStatus = "AutoApproved",
                Reason = (string)null,
                Data = request.Changes.Single(c => c.ChangeType == "Firstname").Data
            },
            new
            {
                ChangeType = "Lastname",
                ApprovalStatus = "AutoApproved",
                Reason = (string)null,
                Data = request.Changes.Single(c => c.ChangeType == "Lastname").Data
            }
        });
    }

    [Test]
    public async Task CocApprovals_EchoesSubmittedPrices_AndOmitsDataOnPricesChange()
    {
        var request = CreateCocApprovalRequest(true);
        request.Prices =
        [
            new PriceRecord { TrainingPrice = 1000m, AssessmentPrice = 500m, EffectiveFrom = new DateTime(2026, 7, 8) },
            new PriceRecord { TrainingPrice = 2000m, AssessmentPrice = 1000m, EffectiveFrom = new DateTime(2026, 9, 1) }
        ];
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .With(x => x.CocApprovalDetails, cocApprovalDetails)
            .With(x => x.Action, AggregationAction.CreateNew)
            .Create();
        var commandResult = new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Pending,
            Items =
            [
                new CocUpdateResult
                {
                    Field = CocChangeField.Prices,
                    Status = CocApprovalItemStatus.Pending
                }
            ]
        };

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);

        var result = await _controller.PutApprovals(request.LearningKey, request);

        var response = result.Should().BeOfType<CreatedResult>().Subject.Value.Should().BeOfType<PutApprovalsResponse>().Subject;
        response.Prices.Should().BeSameAs(request.Prices);
        response.Changes.Should().ContainSingle(x => x.ChangeType == "Prices")
            .Which.Data.Should().BeNull();
        response.Changes.Single(x => x.ChangeType == "Prices").ApprovalStatus.Should().Be("EmployerApprovalRequested");
        response.Changes.Should().Contain(x => x.ChangeType == "Firstname" && x.Data.Old == "John" && x.Data.New == "Joe");
    }

    [Test]
    public async Task CocApprovals_Processes_A_Request_And_Pending_Request_Exists_Then_ReturnsResponseAndDoBAsAutoApproved()
    {
        var request = CreateCocApprovalRequest(false);
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .With(x => x.CocApprovalDetails, cocApprovalDetails)
            .With(x => x.Action, AggregationAction.SupersedePrevious).Create();
        var commandResult = CreateCocResult();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);
        commandResult.Items.ForEach(i => i.Status = CocApprovalItemStatus.Pending);

        var result = await _controller.PutApprovals(request.LearningKey, request);

        result.Should().NotBeNull();
        var jsonResult = result.Should().BeOfType<CreatedResult>().Subject;
        jsonResult.StatusCode.Should().Be(201);
        var response = jsonResult.Value.Should().BeOfType<PutApprovalsResponse>().Subject;
        response.Prices.Should().BeNull();
        response.Changes.Should().HaveCount(3);
        response.Changes.Should().ContainSingle(x =>
            x.ChangeType == "Firstname" && x.ApprovalStatus == "EmployerApprovalRequested" && x.Reason == null
            && x.Data.Old == "John" && x.Data.New == "Joe");
        response.Changes.Should().ContainSingle(x =>
            x.ChangeType == "Lastname" && x.ApprovalStatus == "EmployerApprovalRequested" && x.Reason == null
            && x.Data.Old == "Smith" && x.Data.New == "Jones");
        response.Changes.Should().ContainSingle(x =>
            x.ChangeType == "DoB" && x.ApprovalStatus == "AutoApproved" && x.Reason == null
            && x.Data.Old == "1999-01-01" && x.Data.New == "1998-02-01");
    }

    [Test]
    public async Task CocApprovals_Processes_A_Request_Which_Cancels_Pending_Request_Then_ReturnsResponse()
    {
        var request = CreateCocApprovalRequest(true);
        request.Prices = null;
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .Without(x => x.CocApprovalDetails)
            .With(x => x.Action, AggregationAction.CancelPrevious).Create();
        var commandResult = _fixture.Create<CocApprovalResult>();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);

        var result = await _controller.PutApprovals(request.LearningKey, request);

        result.Should().NotBeNull();
        var jsonResult = result.Should().BeOfType<OkObjectResult>().Subject;
        jsonResult.StatusCode.Should().Be(200);
        var response = jsonResult.Value.Should().BeOfType<PutApprovalsResponse>().Subject;
        response.Prices.Should().BeNull();
        response.Changes.Should().BeEquivalentTo(request.Changes.Select(x => new
        {
            x.ChangeType,
            ApprovalStatus = "AutoApproved",
            Reason = (string)null,
            x.Data
        }).ToList());
    }

    private CocApprovalRequest CreateCocApprovalRequest(bool withOnlyNameValues)
    {
        var request = new CocApprovalRequest
        {
            LearningKey = Guid.NewGuid(),
            ApprenticeshipId = 12345,
            LearningType = "Standard",
            UKPRN = "12345678",
            ULN = "87654321",
            ApprovedUri = "http://example.com/approval",
            Changes = new List<CocApprovalFieldChange>
            {
                new CocApprovalFieldChange
                {
                    ChangeType = "Firstname",
                    Data = new CocData
                    {
                        Old = "John",
                        New = "Joe"
                    }
                },
                new CocApprovalFieldChange
                {
                    ChangeType = "Lastname",
                    Data = new CocData
                    {
                        Old = "Smith",
                        New = "Jones"
                    }
                },
                new CocApprovalFieldChange
                {
                    ChangeType = "DoB",
                    Data = new CocData
                    {
                        Old = "1999-01-01",
                        New = "1998-02-01"
                    }
                }
            }
        };

        if (withOnlyNameValues)
        {
            request.Changes = request.Changes.Where(x => x.ChangeType == "Firstname" || x.ChangeType == "Lastname").ToList();
        }

        return request;
    }

    private CocApprovalResult CreateCocResult()
    {
        return new CocApprovalResult
        {
            Status = CocApprovalResultStatus.Pending,
            Items = new List<CocUpdateResult>
            {
                new CocUpdateResult
                {
                    Field = CocChangeField.Firstname,
                    Status = CocApprovalItemStatus.Pending,
                    Reason = null
                },
                new CocUpdateResult
                {
                    Field = CocChangeField.Lastname,
                    Status = CocApprovalItemStatus.Pending,
                    Reason = null
                }
            }
        };
    }
}
