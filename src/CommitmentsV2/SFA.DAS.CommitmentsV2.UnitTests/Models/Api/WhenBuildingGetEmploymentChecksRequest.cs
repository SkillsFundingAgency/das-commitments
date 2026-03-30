using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Api
{
    public class WhenBuildingGetEmploymentChecksRequest
    {
        [Test]
        public void Then_The_Url_Is_Correctly_Constructed()
        {
            //Arrange
            var actual = new GetEmploymentChecksRequest(new List<long> { 265864, 265866, 265868 });

            //Assert
            actual.GetUrl.Should().Be("EmploymentChecks?apprenticeshipIds=265864&apprenticeshipIds=265866&apprenticeshipIds=265868");
        }
    }
}
