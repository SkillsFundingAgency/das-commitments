using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests;

public class ApprovedApprenticeshipsTests
{
    [Test, MoqAutoData]
    public async Task GetApprovedApprenticeshipsForUln_ReturnsQueryResults(
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] ApprenticeshipController sut,
        GetSupportApprovedApprenticeshipsQueryResult expectedResult,
        string uln)
    {
        mediatorMock
            .Setup(m => m.Send(
                It.Is<GetSupportApprovedApprenticeshipsQuery>(q =>
                    q.CohortId == null && q.Uln == uln && q.ApprenticeshipId == null),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

        var actual = await sut.GetApprovedApprenticeshipForUln(uln);

        actual.As<OkObjectResult>().Value.As<GetSupportApprovedApprenticeshipsQueryResult>().Should().NotBeNull();
        actual.As<OkObjectResult>().Value.As<GetSupportApprovedApprenticeshipsQueryResult>().ApprovedApprenticeships
            .Should().BeSameAs(expectedResult.ApprovedApprenticeships);
    }

    [Test, MoqAutoData]
    public async Task GetApprovedApprenticeship_ReturnsAMatch(
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] ApprenticeshipController sut,
        GetSupportApprovedApprenticeshipsQueryResult expectedResult,
        long apprenticeshipId)
    {
        mediatorMock
            .Setup(m => m.Send(
                It.Is<GetSupportApprovedApprenticeshipsQuery>(q =>
                    q.CohortId == null && q.Uln == null && q.ApprenticeshipId == apprenticeshipId),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

        var actual = await sut.GetApprovedApprenticeship(apprenticeshipId);

        actual.As<OkObjectResult>().Value.As<SupportApprenticeshipDetails>().Should().NotBeNull();
        actual.As<OkObjectResult>().Value.As<SupportApprenticeshipDetails>().Should()
            .BeSameAs(expectedResult.ApprovedApprenticeships.First());
    }

    [Test, MoqAutoData]
    public async Task GetApprovedApprenticeship_ReturnsNoMatch(
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] ApprenticeshipController sut,
        long apprenticeshipId)
    {
        var expectedResult = new GetSupportApprovedApprenticeshipsQueryResult
            {ApprovedApprenticeships = new List<SupportApprenticeshipDetails>()};

        mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetSupportApprovedApprenticeshipsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var actual = await sut.GetApprovedApprenticeship(apprenticeshipId);

        actual.As<NotFoundResult>().Should().NotBeNull();
    }
}
