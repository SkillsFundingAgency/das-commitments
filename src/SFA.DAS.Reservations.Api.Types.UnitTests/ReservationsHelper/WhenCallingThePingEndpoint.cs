using System;
using System.Threading.Tasks;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types.UnitTests.ReservationsHelper
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingThePingEndpoint
    {
        [Test]
        public void ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new ReservationHelperTestFixtures()
                                .WithBaseUrlForReservations("https://somehost");

            fixture.AssertUrlBuiltCorrectly();
        }

        private class ReservationHelperTestFixtures
        {
            private readonly ValidationReservationMessage _request;
            private readonly ReservationHelper _helper;
            private readonly ReservationsClientApiConfiguration _configuration;

            public ReservationHelperTestFixtures()
            {
                _configuration = new ReservationsClientApiConfiguration();
                _helper = new ReservationHelper(_configuration);

                var autoFixture = new Fixture();
                _request = new ValidationReservationMessage
                {
                    CourseCode = autoFixture.Create<string>(),
                    ReservationId = autoFixture.Create<Guid>(),
                    StartDate = autoFixture.Create<DateTime>()
                };
            }

            public ReservationHelperTestFixtures WithBaseUrlForReservations(string url)
            {
                _configuration.ApiBaseUrl = url;
                return this;
            }

            public void AssertUrlBuiltCorrectly()
            {
                var url = MakeCall();
                var expectedUrl = _configuration.ApiBaseUrl + "/ping";
                
                var expectedUri = new Uri(url, UriKind.Absolute);
                var actualUri = new Uri(expectedUrl, UriKind.Absolute);

                Assert.AreEqual(expectedUri.Host, actualUri.Host, "Host is wrong");
                Assert.AreEqual(expectedUri.AbsolutePath, actualUri.AbsolutePath, "Path is wrong");
                Assert.AreEqual(expectedUri.Scheme, actualUri.Scheme, "Scheme is wrong");
            }

            private string MakeCall()
            {
                string actualUrl = null;

                _helper.Ping(url  =>  
                {
                    actualUrl = url;
                    return Task.CompletedTask;
                });

                return actualUrl;
            }
        }
    }
}