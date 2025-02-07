using Microsoft.AspNetCore.Http;
using SFA.DAS.CommitmentsV2.Api.Middleware;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Middleware;

[TestFixture]
public class SecurityHeadersMiddlewareTests
{
    private Mock<RequestDelegate> _nextMock;
    private SecurityHeadersMiddleware _middleware;
    
    [SetUp]
    public void Setup()
    {
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new SecurityHeadersMiddleware(_nextMock.Object);
    }

    [Test]
    public async Task InvokeAsync_ShouldAddSecurityHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        
        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.XFrameOptions.Should().BeEquivalentTo("DENY");
        context.Response.Headers.XContentTypeOptions.Should().BeEquivalentTo("nosniff");
        context.Response.Headers.XFrameOptions.Should().BeEquivalentTo("DENY");
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"].Should().BeEquivalentTo("none");
        context.Response.Headers.ContentSecurityPolicy.Should().BeEquivalentTo("default-src *; script-src *; connect-src *; img-src *; style-src *; object-src *;");

        _nextMock.Verify(next => next(context), Times.Once);
    }
}
