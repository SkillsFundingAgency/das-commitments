using System.IO;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.CookieStorageServiceTests
{
    public class WhenCallingCreate
    {
        [Test, MoqAutoData, Ignore("wip")]
        public void Then_Stuff(
            ChocolateChipCookie cookie,
            string cookieName,
            int expiryDays,
            Mock<IResponseCookies> mockResponseCookies,
            Mock<HttpResponse> mockResponse,
            [Frozen] Mock<HttpContext> mockHttpContext,
            [Frozen] Mock<IDataProtectionProvider> mockDataProtectionProvider,
            CookieStorageService<ChocolateChipCookie> cookieStorageService)
        {
            var stream = new MemoryStream();
            mockResponse
                .Setup(response => response.Cookies)
                .Returns(mockResponseCookies.Object);
            mockResponse
                .Setup(response => response.Body)
                .Returns(stream);
            mockHttpContext
                .Setup(context => context.Response)
                .Returns(mockResponse.Object);

            cookieStorageService.Create(cookie, cookieName, expiryDays);
        }


        public class ChocolateChipCookie
        {

        }
    }
}