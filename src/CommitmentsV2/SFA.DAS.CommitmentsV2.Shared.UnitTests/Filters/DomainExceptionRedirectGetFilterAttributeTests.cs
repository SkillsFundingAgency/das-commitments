using System.Collections;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Shared.Filters;
using SFA.DAS.Validation.Mvc.ModelBinding;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Filters;

[TestFixture]
public class DomainExceptionRedirectGetFilterAttributeTests
{
    [Test]
    public void OnDomainException_ShouldReturnRedirectToRouteResult()
    {
        var fixture = new DomainExceptionRedirectGetFilterAttributeTestsFixture();
        fixture.WithDomainException();
        fixture.OnException();
        Assert.That(fixture.ExceptionContext.Result, Is.InstanceOf<RedirectToRouteResult>());
    }

    [Test]
    public void OnDomainException_ShouldSetRouteData()
    {
        var fixture = new DomainExceptionRedirectGetFilterAttributeTestsFixture();
        fixture.WithDomainException();
        fixture.OnException();
        Assert.That(fixture.QueryString, Has.Count.EqualTo(fixture.RouteData.Values.Count));
    }

    [Test]
    public void OnDomainException_TempDataShouldContainValidationErrors()
    {
        var fixture = new DomainExceptionRedirectGetFilterAttributeTestsFixture();
        fixture.WithDomainException();
        fixture.OnException();
        fixture.VerifyTempDataContainsValidationErrors();
    }

    public class DomainExceptionRedirectGetFilterAttributeTestsFixture
    {
        public DomainExceptionRedirectGetFilterAttribute DomainExceptionRedirectGetFilterAttribute { get; set; }
        public ActionExecutingContext ActionExecutingContext { get; set; }
        public ExceptionContext ExceptionContext { get; set; }
        public Dictionary<string, object> ActionParameters { get; set; }
        public Mock<HttpContext> HttpContext { get; set; }
        public Foo Model { get; set; }
        public RouteData RouteData { get; set; }
        public QueryCollection QueryString { get; set; }
        public Mock<IServiceProvider> ServiceProvider { get; set; }
        public Mock<ITempDataDictionaryFactory> TempDataDictionaryFactory { get; set; }
        public TempDataDictionaryMock TempDataDictionary { get; set; }

        public DomainExceptionRedirectGetFilterAttributeTestsFixture()
        {
            ActionParameters = new Dictionary<string, object>();
            HttpContext = new Mock<HttpContext>();
            Model = new Foo { Bar = new Bar() };
            RouteData = new RouteData();

            ActionParameters.Add("model", Model);

            ActionExecutingContext = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = HttpContext.Object,
                    RouteData = RouteData,
                    ActionDescriptor = new ActionDescriptor(),
                },
                new List<IFilterMetadata>(),
                ActionParameters,
                Mock.Of<Controller>());

            ExceptionContext = new ExceptionContext(
                new ActionContext
                {
                    HttpContext = HttpContext.Object,
                    RouteData = RouteData,
                    ActionDescriptor = new ActionDescriptor(),
                },
                new List<IFilterMetadata>());

            QueryString = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["Foo"] = "Foo",
                ["Bar"] = "Bar"
            });

            ServiceProvider = new Mock<IServiceProvider>();
            TempDataDictionaryFactory = new Mock<ITempDataDictionaryFactory>();
            TempDataDictionary = new TempDataDictionaryMock();

            DomainExceptionRedirectGetFilterAttribute = new DomainExceptionRedirectGetFilterAttribute();

            HttpContext.Setup(c => c.Request.Query).Returns(QueryString);
            HttpContext.Setup(c => c.RequestServices).Returns(ServiceProvider.Object);
            ServiceProvider.Setup(p => p.GetService(typeof(ITempDataDictionaryFactory))).Returns(TempDataDictionaryFactory.Object);
            TempDataDictionaryFactory.Setup(f => f.GetTempData(HttpContext.Object)).Returns(TempDataDictionary);
        }

        public void OnException()
        {
            DomainExceptionRedirectGetFilterAttribute.OnException(ExceptionContext);
        }


        public DomainExceptionRedirectGetFilterAttributeTestsFixture WithDomainException()
        {
            var errors = new List<ErrorDetail> { new ErrorDetail("Foo", "Test domain error on Foo") };
            ExceptionContext.Exception = new CommitmentsApiModelException(errors);
            return this;
        }

        public void VerifyTempDataContainsValidationErrors()
        {
            var key = typeof(SerializableModelStateDictionary).FullName;
            var serializedModelState = TempDataDictionary.BackingStore[key] as string;

            var actualErrors = JsonConvert.DeserializeObject<SerializableModelStateDictionary>(serializedModelState);

            var expectedDomainException = (CommitmentsApiModelException)ExceptionContext.Exception;
            Assert.That(actualErrors.Data, Has.Count.EqualTo(expectedDomainException.Errors.Count));

            foreach (var expectedError in expectedDomainException.Errors)
            {
                var actualError = actualErrors.Data.First(x => x.Key == expectedError.Field);
                Assert.That(actualError.ErrorMessages.First(), Is.EqualTo(expectedError.Message));
            }
        }
    }

    public class Foo
    {
        public Bar Bar { get; set; }
    }

    public class Bar
    {
        public int Value { get; set; }
    }

    public class TempDataDictionaryMock : ITempDataDictionary
    {
        public TempDataDictionaryMock()
        {
            BackingStore = new Dictionary<object, object>();
        }

        public Dictionary<object, object> BackingStore { get; set; }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        public object this[string key]
        {
            get => BackingStore[key];
            set => BackingStore[key] = value;
        }

        public ICollection<string> Keys { get; }
        public ICollection<object> Values { get; }
        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Keep()
        {
            throw new NotImplementedException();
        }

        public void Keep(string key)
        {
            throw new NotImplementedException();
        }

        public object Peek(string key)
        {
            throw new NotImplementedException();
        }
    }
}