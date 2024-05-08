using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.CommitmentsV2.Api.Types.Validation;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    [Parallelizable]
    public class ModelStateExtensionsTests
    {
        private ModelStateExtensionsTestsFixture _fixture;
        [SetUp]
        public void SetUp()
        {
            _fixture = new ModelStateExtensionsTestsFixture();
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereIsNoList_NoMessagesShouldBeAdded()
        {
            var exception = new CommitmentsApiModelException(null);

            _fixture.ModelState.AddModelExceptionErrors(exception);

            Assert.That(_fixture.ModelState.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereIsAnEmptyList_NoMessagesShouldBeAdded()
        {
            var exception = new CommitmentsApiModelException(_fixture.Errors);

            _fixture.ModelState.AddModelExceptionErrors(exception);

            Assert.That(_fixture.ModelState.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereAreErrors_TheMessagesShouldBeAppended()
        {
            var exception = new CommitmentsApiModelException(_fixture.Add2ExceptionErrors().Errors);

            _fixture.ModelState.AddModelExceptionErrors(exception);

            Assert.That(_fixture.ModelState.ErrorCount, Is.EqualTo(2));
        }

        [Test]
        public void AddModelExceptionErrors_WhenExistingStateErrors_TheNewErrorsMessagesShouldBeAppended()
        {
            var exception = new CommitmentsApiModelException(_fixture.Add2ExceptionErrors().Errors);

            _fixture.Add1ModelStateErrors().ModelState.AddModelExceptionErrors(exception);

            Assert.That(_fixture.ModelState.ErrorCount, Is.EqualTo(3));
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereAreErrorsAndWeAreMappingfieldNames_TheFieldNamesWillBeMapped()
        {
            var exception = new CommitmentsApiModelException(_fixture.Add2ExceptionErrors().Errors);

            _fixture.ModelState.AddModelExceptionErrors(exception, field => "xxx" + field);

            Assert.That(_fixture.ModelState.ContainsKey("xxxfield2"), Is.True);
        }


    }

    public class ModelStateExtensionsTestsFixture
    {
        public ModelStateDictionary ModelState;
        public List<ErrorDetail> Errors;

        public ModelStateExtensionsTestsFixture()
        {
            ModelState = new ModelStateDictionary();
            Errors = new List<ErrorDetail>();
        }

        public ModelStateExtensionsTestsFixture Add2ExceptionErrors()
        {
            Errors.Add(new ErrorDetail("field1", "Error1"));
            Errors.Add(new ErrorDetail("field2", "Error2"));
            return this;
        }

        public ModelStateExtensionsTestsFixture Add1ModelStateErrors()
        {
            ModelState.AddModelError("modelerror", "model error message");
            return this;
        }

        public CommitmentsApiModelException CreateApiModelExceptionWithErrors()
        {
            return new CommitmentsApiModelException(Errors);
        }
    }
}
