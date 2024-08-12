using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using SFA.DAS.CommitmentsV2.Api.ErrorHandler;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.ErrorHandling;

[TestFixture]
public class FluentValidationToApiErrorResultFactoryTests
{
    private FluentValidationToApiErrorResultFactoryTestsFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new FluentValidationToApiErrorResultFactoryTestsFixture();
    }

    [Test]
    public void WhenFactoryExecuted_ThenShouldNotSetErrorResult()
    {
        var sut = new FluentValidationToApiErrorResultFactory();

        _fixture.ModelState.AddModelError("Foo", "Bar");

        var result = sut.CreateActionResult(_fixture.ActionExecutingContext, null);

        var response = result as BadRequestObjectResult;
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var errorResponse = response.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse.Errors.Count.Should().Be(1);
        errorResponse.Errors[0].Field.Should().Be("Foo");
        errorResponse.Errors[0].Message.Should().Be("Bar");
    }
}

public class FluentValidationToApiErrorResultFactoryTestsFixture
{
    public IHeaderDictionary Headers { get; set; }
    public Mock<HttpContext> HttpContext { get; set; }
    public Mock<TypeInfo> ControllerTypeInfo { get; set; }
    public Mock<MethodInfo> MethodInfo { get; set; }
    public ControllerActionDescriptor ActionDescriptor { get; set; }
    public ModelStateDictionary ModelState { get; set; }
    public ActionContext ActionContext { get; set; }
    public ActionExecutingContext ActionExecutingContext { get; set; }
    public HttpSubStatusCode DomainExceptionHttpSubStatusCode { get; set; }
    public string DomainExceptionHttpSubStatusCodeHeaderValue { get; set; }

    public FluentValidationToApiErrorResultFactoryTestsFixture()
    {
        Headers = new HeaderDictionary(new Dictionary<string, StringValues>());
        HttpContext = new Mock<HttpContext>();
        ControllerTypeInfo = new Mock<TypeInfo>();
        MethodInfo = new Mock<MethodInfo>();
        
        ActionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = Guid.NewGuid().ToString(),
            ControllerTypeInfo = ControllerTypeInfo.Object,
            ActionName = Guid.NewGuid().ToString(),
            MethodInfo = MethodInfo.Object
        };

        ModelState = new ModelStateDictionary();
        ActionContext = new ActionContext(HttpContext.Object, new RouteData(),  ActionDescriptor, ModelState);
        ActionExecutingContext = new ActionExecutingContext(ActionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
        DomainExceptionHttpSubStatusCode = HttpSubStatusCode.DomainException;
        DomainExceptionHttpSubStatusCodeHeaderValue = ((int)DomainExceptionHttpSubStatusCode).ToString();

        HttpContext.Setup(c => c.Response.Headers).Returns(Headers);
    }
}