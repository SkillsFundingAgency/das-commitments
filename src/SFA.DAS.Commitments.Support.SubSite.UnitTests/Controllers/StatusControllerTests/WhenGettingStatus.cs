//using Microsoft.AspNetCore.Mvc;
//using NUnit.Framework;
//using SFA.DAS.Commitments.Support.SubSite.Controllers;
//using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
//using SFA.DAS.Commitments.Support.SubSite.Models;

//namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.StatusControllerTests
//{
//    [TestFixture]
//    public class WhenGettingStatus
//    {
//        [Test]
//        public void ShouldReturnStatusModel()
//        {
//            var sut = new StatusController();
//            ActionResult<ServiceStatusViewModel> result = sut.Get();

//            Assert.IsNotNull(result);
//            Assert.IsNotNull(result.Value);
//            Assert.AreEqual(ApplicationConstants.ServiceName, result.Value.ServiceName);
//        }
//    }
//}