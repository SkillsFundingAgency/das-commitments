using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Authentication;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class WhoAmIControllerTests
    {
        [TestCase(Role.Employer)]
        [TestCase(Role.Provider)]
        [TestCase(new object[] { Role.Employer, Role.Provider })]
        public void WhoAmI_WhenRequestReceived_ThenShouldSendResponse(params string[] roles)
        {
            var fixture = new WhoAmIControllerTestsFixture();
            fixture.SetRoles(roles);
            
            var result = fixture.WhoAmI();

            result.Should().NotBeNull()
                .And.BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<WhoAmIResponse>()
                .Which.Roles.Should().BeEquivalentTo(roles);
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
                AuthenticationService.Setup(s => s.GetAllUserRoles()).Returns(roles);
            }
            
            return this;
        }
    }
}