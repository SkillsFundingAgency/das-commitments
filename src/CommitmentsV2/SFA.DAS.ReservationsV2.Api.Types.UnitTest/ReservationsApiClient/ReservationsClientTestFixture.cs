using System;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Types.UnitTests.ReservationsApiClient;

public class ReservationsClientTestFixtures
{
    protected readonly Reservations.Api.Types.ReservationsApiClient ReservationsApiClient;
    protected readonly Mock<IHttpHelper> HttpHelper;
    protected readonly ReservationsClientApiConfiguration Config;
    protected readonly Fixture AutoFixture;

    public ReservationsClientTestFixtures()
    {
        HttpHelper = new Mock<IHttpHelper>();
        Config = new ReservationsClientApiConfiguration
        {
            ApiBaseUrl = "https://somehost"
        };

        AutoFixture = new Fixture();

        ReservationsApiClient = new Reservations.Api.Types.ReservationsApiClient(Config, HttpHelper.Object);
    }

    protected static bool IsSameUri(string expected, string actual)
    {
        var expectedUri = new Uri(expected, UriKind.Absolute);
        var actualUri = new Uri(actual, UriKind.Absolute);

        Assert.Multiple(() =>
        {
            Assert.That(actualUri.Host, Is.EqualTo(expectedUri.Host), "Host is wrong");
            Assert.That(actualUri.AbsolutePath, Is.EqualTo(expectedUri.AbsolutePath), "Path is wrong");
            Assert.That(actualUri.Scheme, Is.EqualTo(expectedUri.Scheme), "Scheme is wrong");
        });

        return true;
    }
}