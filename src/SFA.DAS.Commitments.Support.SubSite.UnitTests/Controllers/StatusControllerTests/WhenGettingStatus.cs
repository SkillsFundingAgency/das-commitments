using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.StatusControllerTests
{

    [TestFixture]
    public class WhenGettingStatus
    {

        [Test]
        public async Task ShouldReturnStatusModel()
        {
            var sut = new StatusController();
            var result = await sut.Get() as OkNegotiatedContentResult<ServiceStatusViewModel>;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.IsFalse(String.IsNullOrWhiteSpace(result.Content.ServiceName));
        }


    }
}
