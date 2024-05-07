using AutoFixture.NUnit3;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Api
{
    public class WhenBuildingGetProvidersRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Constructed()
        {
            //Arrange
            var actual = new GetProvidersRequest();
            
            //Assert
            actual.GetUrl.Should().Be("providers");
        }
    }
}