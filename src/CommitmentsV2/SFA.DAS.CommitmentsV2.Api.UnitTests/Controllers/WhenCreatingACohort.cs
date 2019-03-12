using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class WhenCreatingACohort
    {
        [Test]
        public void ThenACreateCohortResponseIsReturnedOnSuccessfulCreation()
        {
            //Arrange
            var controller = new CohortController();
            //Act
            var actual = controller.Post(new CreateCohortRequest());
            //Assert
            Assert.IsInstanceOf(typeof(CreateCohortResponse), actual);
        }
    }
}