using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using SFA.DAS.CommitmentsV2.Shared.ModelBinding;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.ModelBinding
{
    [TestFixture]
    public class StringModelBinderTests
    {
        private StringModelBinder _binder;

        [SetUp]
        public void Setup()
        {
            _binder = new StringModelBinder(new SimpleTypeModelBinder(typeof(string), Mock.Of<ILoggerFactory>()));
        }

        [TestCase("test", "test")]
        [TestCase(" test", "test")]
        [TestCase("test", "test")]
        [TestCase(" test ", "test")]
        [TestCase("test this", "test this")]
        [TestCase("test\tthis", "test this")]
        [TestCase("\ttest ", "test")]
        [TestCase("\t test \t", "test")]
        public async Task BindModel_Produces_Expected_Result(string inputValue, string expectedResult)
        {
            var formCollection = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    { "String", new StringValues(inputValue) }
                });
            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(string));

            await _binder.BindModelAsync(context);

            var resultModel = context.Result.Model as string;

            Assert.That(resultModel, Is.EqualTo(expectedResult));
        }
        private static DefaultModelBindingContext GetBindingContext(IValueProvider valueProvider, Type modelType)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = modelType.Name,
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
            return bindingContext;
        }
    }
}
