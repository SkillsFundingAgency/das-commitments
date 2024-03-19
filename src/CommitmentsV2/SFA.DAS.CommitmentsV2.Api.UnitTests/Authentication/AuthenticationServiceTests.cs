using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Authentication
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [TestCase(Party.Employer, Role.Employer)]
        [TestCase(Party.Provider, Role.Provider)]
        public void GetValidUserParty(Party expectedParty, params string[] roles)
        {
            // arrange
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            Mock<ClaimsPrincipal> userMock = new Mock<ClaimsPrincipal>();

            httpContextAccessorMock
                .Setup(hca => hca.HttpContext)
                .Returns(httpContextMock.Object);

            httpContextMock
                .Setup(hcm => hcm.User)
                .Returns(userMock.Object);

            userMock
                .Setup(um => um.IsInRole(It.IsAny<string>()))
                .Returns<string>(roles.Contains);

            var sut = new AuthenticationService(httpContextAccessorMock.Object);

            // act
            var actualRole = sut.GetUserParty();

            // assert
            Assert.That(actualRole, Is.EqualTo(expectedParty));
        }

        [TestCase(Role.Provider, Role.Employer)]
        [TestCase("someOtherRole")]
        public void GetInvalidUserParty(params string[] roles)
        {
            // arrange
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            Mock<ClaimsPrincipal> userMock = new Mock<ClaimsPrincipal>();

            httpContextAccessorMock
                .Setup(hca => hca.HttpContext)
                .Returns(httpContextMock.Object);

            httpContextMock
                .Setup(hcm => hcm.User)
                .Returns(userMock.Object);

            userMock
                .Setup(um => um.IsInRole(It.IsAny<string>()))
                .Returns<string>(roles.Contains);

            var sut = new AuthenticationService(httpContextAccessorMock.Object);

            // act & assert
            Assert.Throws<ArgumentException>(() => sut.GetUserParty());
        }
    }
}
