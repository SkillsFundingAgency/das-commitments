using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class PingControllerTests
    {
        [Test]
        public void Ping_WhenRequestReceived_ThenShouldSendResponse()
        {
            var f = new PingControllerTestsFixture();
            var r = f.Ping();
            
            r.Should().NotBeNull().And.BeOfType<OkResult>();
        }
    }

    public class PingControllerTestsFixture
    {
        public PingController Controller { get; set; }

        public PingControllerTestsFixture()
        {
            Controller = new PingController();
        }

        public IActionResult Ping()
        {
            return Controller.Ping();
        }
    }
}