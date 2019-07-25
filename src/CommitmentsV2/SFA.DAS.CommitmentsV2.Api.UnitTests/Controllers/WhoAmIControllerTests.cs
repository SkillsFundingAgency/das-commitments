using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class WhoAmIControllerTests : FluentTest<WhoAmIControllerTestsFixture>
    {
        [TestCase(Role.Employer)]
        [TestCase(Role.Provider)]
        public void WhoAmI_WhenUserIsInRole_ThenShouldSendResponse(string role)
        {
            Test(
                f => f.SetRoles(role),
                f => f.WhoAmI(),
                (f, r) => r.Should().NotBeNull()
                    .And.BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeOfType<WhoAmIResponse>()
                    .Which.Role.Should().Be(role));
        }

        [Test]
        public void WhoAmI_WhenUserIsNotInAnyRole_ThenShouldThrowException()
        {
            TestException(
                f => f.WhoAmI(),
                (f, a) => a.Should().Throw<InvalidOperationException>().WithMessage("Client is authenticated with an unknown role"));
        }
        
        [Test]
        public void WhoAmI_WhenUserIsInMultipleRoles_ThenShouldThrowException()
        {
            TestException(
                f => f.SetRoles(Role.Employer, Role.Provider), 
                f => f.WhoAmI(),
                (f, a) => a.Should().Throw<InvalidOperationException>().WithMessage("Client is authenticated with multiple roles"));
        }
    }

    public class WhoAmIControllerTestsFixture
    {
        public Mock<IAuthenticationService> AuthenticationService { get; set; }
        public WhoAmIController Controller { get; set; }

        public WhoAmIControllerTestsFixture()
        {
            AuthenticationService = new Mock<IAuthenticationService>();
            Controller = new WhoAmIController(AuthenticationService.Object);
        }

        public IActionResult WhoAmI()
        {
            return Controller.WhoAmI();
        }

        public WhoAmIControllerTestsFixture SetRoles(params string[] roles)
        {
            foreach (var role in roles)
            {
                AuthenticationService.Setup(s => s.IsUserInRole(role)).Returns(true);
            }
            
            return this;
        }
    }
}