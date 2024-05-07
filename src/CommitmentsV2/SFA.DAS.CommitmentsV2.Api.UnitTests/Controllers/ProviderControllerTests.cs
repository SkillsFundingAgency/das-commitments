using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class ProviderControllerTests
{
    private ProviderControllerTestsFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new ProviderControllerTestsFixture();
    }

    [Test]
    public async Task GetAllProviders_ThenShouldReturnListOfProviders()
    {
        var response = await _fixture.SetGetAllProvidersQueryResult().GetAllProviders();
        var okObjectResult = response as OkObjectResult;
        var getAllProvidersResponse = okObjectResult?.Value as GetAllProvidersResponse;

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(okObjectResult, Is.Not.Null);
            Assert.That(getAllProvidersResponse, Is.Not.Null);
            Assert.That(getAllProvidersResponse.Providers, Has.Count.EqualTo(_fixture.GetAllProvidersQueryResult.Providers.Count));
        });
    }

    [Test]
    public async Task GetProvider_WhenProviderDoesExist_ThenShouldReturnProviderResponse()
    {
        var response = await _fixture.SetGetProviderQueryResult().GetProvider();
        var okObjectResult = response as OkObjectResult;
        var getProviderResponse = okObjectResult?.Value as GetProviderResponse;

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(okObjectResult, Is.Not.Null);
            Assert.That(getProviderResponse, Is.Not.Null);
            Assert.That(getProviderResponse.ProviderId, Is.EqualTo(_fixture.GetProviderQueryResult.ProviderId));
            Assert.That(getProviderResponse.Name, Is.EqualTo(_fixture.GetProviderQueryResult.Name));
        });
    }

    [Test]
    public async Task GetProvider_WhenProviderDoesNotExist_ThenShouldReturnNotFoundResponse()
    {
        var response = await _fixture.GetProvider();
        var notFoundResult = response as NotFoundResult;

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(notFoundResult, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetCommitmentAgreements_ThenShouldReturnListOfCommitmentAgreements()
    {
        var response = await _fixture.SetGetCommitmentAgreementsResult().GetCommitmentAgreements();
        var okObjectResult = response as OkObjectResult;
        var getCommitmentAgreementsResponse = okObjectResult?.Value as GetProviderCommitmentAgreementResponse;

        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(okObjectResult, Is.Not.Null);
            Assert.That(getCommitmentAgreementsResponse, Is.Not.Null);
            Assert.That(getCommitmentAgreementsResponse.ProviderCommitmentAgreement, Has.Count.EqualTo(_fixture.GetProviderCommitmentAgreementQueryResult.Agreements.Count));
        });
    }
}

public class ProviderControllerTestsFixture
{
    public Mock<IMediator> Mediator { get; set; }
    public ProviderController Controller { get; set; }
    public long ProviderId { get; set; }
    public string ProviderName { get; set; }
    public GetProviderQueryResult GetProviderQueryResult { get; set; }
    public GetAllProvidersQueryResult GetAllProvidersQueryResult { get; set; }
    public GetProviderCommitmentAgreementResult GetProviderCommitmentAgreementQueryResult { get; set; }

    public ProviderControllerTestsFixture()
    {
        Mediator = new Mock<IMediator>();
        Controller = new ProviderController(Mediator.Object);
        ProviderId = 1;
        ProviderName = "Foo";
        GetProviderQueryResult = new GetProviderQueryResult(ProviderId, ProviderName);
        GetAllProvidersQueryResult = GetAllProvidersResult();
        GetProviderCommitmentAgreementQueryResult = GetProviderCommitmentAgreementResult();
    }

    public Task<IActionResult> GetAllProviders()
    {
        return Controller.GetAllProviders();
    }

    public Task<IActionResult> GetProvider()
    {
        return Controller.GetProvider(ProviderId);
    }

    public Task<IActionResult> GetCommitmentAgreements()
    {
        return Controller.GetCommitmentAgreements(ProviderId);
    }

    public ProviderControllerTestsFixture SetGetProviderQueryResult()
    {
        Mediator.Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.ProviderId == ProviderId), CancellationToken.None))
            .ReturnsAsync(GetProviderQueryResult);

        return this;
    }

    public ProviderControllerTestsFixture SetGetAllProvidersQueryResult()
    {
        Mediator.Setup(m => m.Send(It.IsAny<GetAllProvidersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetAllProvidersQueryResult);

        return this;
    }

    public ProviderControllerTestsFixture SetGetCommitmentAgreementsResult()
    {
        Mediator.Setup(m => m.Send(It.IsAny<GetProviderCommitmentAgreementQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetProviderCommitmentAgreementQueryResult);

        return this;
    }

    private static GetAllProvidersQueryResult GetAllProvidersResult()
    {
        return new GetAllProvidersQueryResult(
        [
            new() { Ukprn = 10000001, Name = "Provider 1" },
            new() { Ukprn = 10000002, Name = "Provider 2" },
            new() { Ukprn = 10000003, Name = "Provider 3" }
        ]);
    }

    private static GetProviderCommitmentAgreementResult GetProviderCommitmentAgreementResult()
    {
        return new GetProviderCommitmentAgreementResult(
            new List<ProviderCommitmentAgreement>
            {
                new()
                {
                    AccountLegalEntityPublicHashedId = "A001",
                    LegalEntityName = "A001",
                },
                new()
                {
                    AccountLegalEntityPublicHashedId = "B001",
                    LegalEntityName = "B001",
                },
                new()
                {
                    AccountLegalEntityPublicHashedId = "C001",
                    LegalEntityName = "C001"
                }
            });
    }
}