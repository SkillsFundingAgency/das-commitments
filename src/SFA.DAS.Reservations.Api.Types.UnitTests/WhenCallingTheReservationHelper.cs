using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingReservationHelper
    {

        [Test]
        public void ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new ReservationHelperTestFixtures()
                                .WithBaseUrlForReservations("https://somehost");

            fixture.AssertUrlBuiltCorrectly();
        }

        [Test]
        public void ThenTheRequestDataIsCorrectlyFormed()
        {
            var fixture = new ReservationHelperTestFixtures()
                .WithBaseUrlForReservations("https://somehost");

            fixture.AssertDataBuiltCorrectly();
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
                var actualResults = MakeCall();
                var expectedUrl = _configuration.ApiBaseUrl + $"/api/reservations/validate/{_request.ReservationId}";

                var expectedUri = new Uri(actualResults.url, UriKind.Absolute);
                var actualUri = new Uri(expectedUrl, UriKind.Absolute);

                Assert.AreEqual(expectedUri.Host, actualUri.Host, "Host is wrong");
                Assert.AreEqual(expectedUri.AbsolutePath, actualUri.AbsolutePath, "Path is wrong");
                Assert.AreEqual(expectedUri.Scheme, actualUri.Scheme, "Scheme is wrong");
            }

            public void AssertDataBuiltCorrectly()
            {
                var actualResults = MakeCall();
                
                AssertHasPropertyWithValue(actualResults.data, "StartDate","");
                AssertHasPropertyWithValue(actualResults.data, "CourseCode", "");
            }

            private void AssertHasPropertyWithValue(object data, string propertyName, string value)
            {
                var property = data.GetType().GetProperty(propertyName);

                Assert.IsNotNull(property, $"Data does not have a property named {propertyName}");

                var actualQueryValue = property.GetValue(data);
                Assert.IsNotNull(property, $"Data has a property named {propertyName} but it does not have a value");

                var queryValue = value as string;
                Assert.IsNotNull(queryValue, $"Data has a property named {propertyName} but the value it has is not a string (it is a {value.GetType().Name})");
            }

            private (string url, object data) MakeCall()
            {
                string actualUrl = null;
                object actualData = null;

                _helper.ValidateReservation(_request, (url, data) =>  
                {
                    actualUrl = url;
                    actualData = data;
                    return Task.FromResult(new ReservationValidationResult());
                });

                return (actualUrl, actualData);
            }
        }
    }
}