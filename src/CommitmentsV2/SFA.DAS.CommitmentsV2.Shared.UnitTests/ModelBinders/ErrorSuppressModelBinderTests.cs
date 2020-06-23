using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.ModelBinders;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.ModelBinders
{

    [TestFixture]
    public class ErrorSuppressModelBinderTests
    {
        private ErrorSuppressModelBinderTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ErrorSuppressModelBinderTestsFixture();
        }

        [Test]
        public async Task Valid_Value_Binds_Correctly()
        {
            await _fixture.Bind();
            _fixture.VerifyBindingSuccess();
        }

        [Test]
        public async Task Invalid_Value_Does_Not_Bind()
        {
            _fixture.WithInvalidInput();
            await _fixture.Bind();
            _fixture.VerifyBindingFailure();
        }

        [Test]
        public async Task Invalid_Value_Does_Not_Add_Error_To_ModelState()
        {
            _fixture.WithInvalidInput();
            await _fixture.Bind();
            _fixture.VerifyNoErrorsInModelState();
        }

        private class ErrorSuppressModelBinderTestsFixture
        {
            private readonly ErrorSuppressModelBinder _binder;
            private ModelBindingResult _result;
            private readonly ModelStateDictionary _modelStateDictionary;
            private readonly Mock<DefaultModelBindingContext> _context;
            private readonly Mock<IValueProvider> _valueProvider;

            private const string InvalidInput = "XXX";
            private const string ValidInput = "123";

            public ErrorSuppressModelBinderTestsFixture()
            {
                _binder = new ErrorSuppressModelBinder();
                _context = new Mock<DefaultModelBindingContext>();
                _valueProvider = new Mock<IValueProvider>();
                _modelStateDictionary = new ModelStateDictionary();

                _context.Setup(x => x.ModelName).Returns("Test");
                _context.Setup(x => x.ModelType).Returns(typeof(int?));
                _context.Setup(x => x.Model).Returns(new BindingTestClass());

                var valueProviderResult = new ValueProviderResult(ValidInput);
                _valueProvider.Setup(x => x.GetValue("Test")).Returns(valueProviderResult);
                _context.Setup(x => x.ValueProvider).Returns(_valueProvider.Object);
                _context.Setup(x => x.ModelState).Returns(_modelStateDictionary);

                _result = new ModelBindingResult();
                _context.SetupSet(p => p.Result = It.IsAny<ModelBindingResult>()).Callback<ModelBindingResult>(r => _result = r);
            }

            public ErrorSuppressModelBinderTestsFixture WithInvalidInput()
            {
                var valueProviderResult = new ValueProviderResult(InvalidInput);
                _valueProvider.Setup(x => x.GetValue("Test")).Returns(valueProviderResult);
                return this;
            }

            public async Task Bind()
            {
                await _binder.BindModelAsync(_context.Object);
            }

            public void VerifyNoErrorsInModelState()
            {
                Assert.AreEqual(0, _modelStateDictionary.ErrorCount);
            }

            public void VerifyBindingFailure()
            {
                var expectedResult = ModelBindingResult.Failed();
                Assert.AreEqual(expectedResult, _result);
            }

            public void VerifyBindingSuccess()
            {
                var expectedResult = ModelBindingResult.Success(int.Parse(ValidInput));
                Assert.AreEqual(expectedResult, _result);
            }
        }

        private class BindingTestClass
        {
            public int? Test { get; set; }
        }
    }
}
