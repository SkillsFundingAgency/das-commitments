using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ReservationsV2.Api.Types.UnitTests.ReservationsApiClient;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class WhenCreatingAutoReservations
{
    [Test]
    public async Task ThenTheRequestUriIsCorrectlyFormed()
    {
        var fixture = new WhenCreatingAutoReservationsFixtures();
        await fixture.CreateAutoReservation();
        fixture.AssertUriCorrectlyFormed();
    }

    [Test]
    public async Task ThenTheRequestPayloadIsPassedInCorrectly()
    {
        var fixture = new WhenCreatingAutoReservationsFixtures();
        await fixture.CreateAutoReservation();
        fixture.AssertPayloadIsPassedInCorrectly();
    }
}

public class WhenCreatingAutoReservationsFixtures : ReservationsClientTestFixtures
{
    private readonly CreateAutoReservationRequest _request;

    public WhenCreatingAutoReservationsFixtures()
    {
        HttpHelper.Setup(x => x.PostAsJson<CreateAutoReservationRequest, CreateAutoReservationResponse>(It.IsAny<string>(),
                It.IsAny<CreateAutoReservationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateAutoReservationResponse { Id = Guid.NewGuid()});

        _request = AutoFixture.Create<CreateAutoReservationRequest>();
    }

    public Task CreateAutoReservation()
    {
        return ReservationsApiClient.CreateAutoReservation(_request, CancellationToken.None);
    }

    public void AssertUriCorrectlyFormed()
    {
        var expectedUrl = $"{Config.ApiBaseUrl}/api/accounts/{_request.AccountId}/reservations";

        HttpHelper.Verify(x => x.PostAsJson<CreateAutoReservationRequest, CreateAutoReservationResponse>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
            It.IsAny<CreateAutoReservationRequest>(), It.IsAny<CancellationToken>()));
    }

    public void AssertPayloadIsPassedInCorrectly()
    {
        HttpHelper.Verify(x => x.PostAsJson<CreateAutoReservationRequest, CreateAutoReservationResponse>(It.IsAny<string>(), _request, It.IsAny<CancellationToken>()));
    }
}