using SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOverlap;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetEmailOverlap;

[TestFixture]
public class ValidateEmailOverlapQueryHandlerTest
{ 

    [Test]
    public async Task Handle_Test()
    {
        var fixtures = new GetEmailOverlapsQueryHandlerTestFixtures();
        var result = await fixtures.GetResult(new ValidateEmailOverlapQuery() { CohortId = 1, DraftApprenticeshipId = 1, Email = "Test@test.com", EndDate = DateTime.Now.AddYears(1).Date.ToShortDateString(), StartDate = DateTime.Now.Date.ToShortDateString() });

        Assert.That(result.OverlapStatus==fixtures.OverlapResults.OverlapStatus, Is.True);
    }
}

public class GetEmailOverlapsQueryHandlerTestFixtures
{
    private readonly Fixture _autoFixture;
    public GetEmailOverlapsQueryHandlerTestFixtures()
    {
        _autoFixture = new Fixture();
        OverlapResults = _autoFixture.Create<EmailOverlapCheckResult>();

        OverlapCheckServiceMock = new Mock<IOverlapCheckService>();
        OverlapCheckServiceMock.Setup(x => x.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(),
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OverlapResults);
    }

    public EmailOverlapCheckResult OverlapResults { get; set; }

    private Mock<IOverlapCheckService> OverlapCheckServiceMock { get; set; }

    public Task<ValidateEmailOverlapQueryResult> GetResult(ValidateEmailOverlapQuery query)
    {
        var handler = new ValidateEmailOverlapQueryHandler(OverlapCheckServiceMock.Object);

        return handler.Handle(query, CancellationToken.None);
    }
}

