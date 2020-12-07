using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Infrastructure.Api.Requests;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Api.Requests
{
    public class WhenMakingGetEpaoOrganisationsRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Constructed()
        {
            //Act
            var actual = new GetEpaoOrganisationsRequest();
            
            //Assert
            actual.GetUrl.Should().Be("epaos");
        }
    }
}