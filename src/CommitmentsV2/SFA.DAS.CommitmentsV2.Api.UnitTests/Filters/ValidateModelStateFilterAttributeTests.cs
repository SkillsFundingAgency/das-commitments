using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using SFA.DAS.CommitmentsV2.Api.Filters;
using SFA.DAS.CommitmentsV2.Api.Types.Http;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Filters
{
    [TestFixture]
    public class ValidateModelStateFilterAttributeTests
    {
        private ValidateModelStateFilterTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ValidateModelStateFilterTestsFixture();
        }

        [Test]
        public void OnActionExecuting_WhenModelStateIsValid_ThenShouldNotSetResult()
        {
            _fixture.OnActionExecuting();

            Assert.That(_fixture.ActionExecutingContext.Result, Is.Null);
        }

        [Test]
        public void OnActionExecuting_WhenModelStateIsNotValid_ThenShouldSetSubStatusCodeHeader()
        {
            _fixture.SetInvalidModelState().OnActionExecuting();

            Assert.That(_fixture.Headers[HttpHeaderNames.SubStatusCode], Is.EqualTo(_fixture.DomainExceptionHttpSubStatusCodeHeaderValue));
        }

        [Test]
        public void OnActionExecuting_WhenModelStateIsNotValid_ThenShouldSetBadRequestObjectResult()
        {
            _fixture.SetInvalidModelState().OnActionExecuting();

            var badRequestObjectResult = _fixture.ActionExecutingContext.Result as BadRequestObjectResult;
            var errorResponse = badRequestObjectResult?.Value as ErrorResponse;

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.ActionExecutingContext.Result, Is.Not.Null);
                Assert.That(badRequestObjectResult, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(badRequestObjectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
                Assert.That(errorResponse, Is.Not.Null);
            });
            Assert.That(errorResponse.Errors.Exists(e => e.Field == "Foo" && e.Message == "Bar"));
        }
    }

    public class ValidateModelStateFilterTestsFixture
    {
        public IHeaderDictionary Headers { get; set; }
        public Mock<HttpContext> HttpContext { get; set; }
        public Mock<TypeInfo> ControllerTypeInfo { get; set; }
        public Mock<MethodInfo> MethodInfo { get; set; }
        public ControllerActionDescriptor ActionDescriptor { get; set; }
        public ModelStateDictionary ModelState { get; set; }
        public ActionContext ActionContext { get; set; }
        public ActionExecutingContext ActionExecutingContext { get; set; }
        public ValidateModelStateFilterAttribute ValidateModelStateFilterAttribute { get; set; }
        public HttpSubStatusCode DomainExceptionHttpSubStatusCode { get; set; }
        public string DomainExceptionHttpSubStatusCodeHeaderValue { get; set; }

        public ValidateModelStateFilterTestsFixture()
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
            ValidateModelStateFilterAttribute = new ValidateModelStateFilterAttribute();
            DomainExceptionHttpSubStatusCode = HttpSubStatusCode.DomainException;
            DomainExceptionHttpSubStatusCodeHeaderValue = ((int)DomainExceptionHttpSubStatusCode).ToString();

            HttpContext.Setup(c => c.Response.Headers).Returns(Headers);
        }

        public void OnActionExecuting()
        {
            ValidateModelStateFilterAttribute.OnActionExecuting(ActionExecutingContext);
        }

        public ValidateModelStateFilterTestsFixture SetInvalidModelState()
        {
            ModelState.AddModelError("Foo", "Bar");

            return this;
        }
    }
}