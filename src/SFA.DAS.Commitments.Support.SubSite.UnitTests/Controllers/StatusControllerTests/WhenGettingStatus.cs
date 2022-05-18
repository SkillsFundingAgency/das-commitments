using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
using SFA.DAS.Commitments.Support.SubSite.Models;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.StatusControllerTests
{
    [TestFixture]
    public class WhenGettingStatus
    {
        [Test]
        public void ShouldReturnStatusModel()
        {
            var sut = new StatusController();
            var viewResult = sut.Get();

            Assert.AreEqual(typeof(OkObjectResult), viewResult.GetType());
            var objectResult = (OkObjectResult)viewResult;
            Assert.AreEqual(200, objectResult.StatusCode);

            var result = objectResult.Value as ServiceStatusViewModel;
            Assert.IsNotNull(result);
            Assert.AreEqual(ApplicationConstants.ServiceName, result.ServiceName);
        }
    }
}