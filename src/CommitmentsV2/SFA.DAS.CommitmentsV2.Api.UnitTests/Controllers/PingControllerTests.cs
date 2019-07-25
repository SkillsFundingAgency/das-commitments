using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class PingControllerTests : FluentTest<PingControllerTestsFixture>
    {
        [Test]
        public void Ping_WhenRequestReceived_ThenShouldSendResponse()
        {
            Test(f => f.Ping(), (f, r) => r.Should().NotBeNull().And.BeOfType<OkResult>());
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