using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Authentication
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [TestCase(Originator.Employer, Role.Employer)]
        [TestCase(Originator.Provider, Role.Provider)]
        [TestCase(Originator.Unknown, Role.Provider, Role.Employer)]
        [TestCase(Originator.Unknown, "someOtherRole")]
        public void GetUserRole(Originator expectedRole, params string[] roles)
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
            var actualRole = sut.GetUserRole();

            // assert
            Assert.AreEqual(expectedRole, actualRole);
        }
    }
}
