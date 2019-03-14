using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
using SFA.DAS.Commitments.Support.SubSite.Models;
using System.Web.Http.Results;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.StatusControllerTests
{
    [TestFixture]
    public class WhenGettingStatus
    {
        [Test]
        public void ShouldReturnStatusModel()
        {
            var sut = new StatusController();
            var result = sut.Get() as OkNegotiatedContentResult<ServiceStatusViewModel>;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.AreEqual(ApplicationConstants.ServiceName, result.Content.ServiceName);
        }
    }
}
