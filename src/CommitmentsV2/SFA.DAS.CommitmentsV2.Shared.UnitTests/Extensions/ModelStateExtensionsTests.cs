using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
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

            Assert.AreEqual(0, _fixture.ModelState.ErrorCount);
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereIsAnEmptyList_NoMessagesShouldBeAdded()
        {
            var exception = new CommitmentsApiModelException(_fixture.Errors);

            _fixture.ModelState.AddModelExceptionErrors(exception);

            Assert.AreEqual(0, _fixture.ModelState.ErrorCount);
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereAreErrors_TheMessagesShouldBeAppended()
        {
            var exception = new CommitmentsApiModelException(_fixture.Add2ExceptionErrors().Errors);

            _fixture.ModelState.AddModelExceptionErrors(exception);

            Assert.AreEqual(2, _fixture.ModelState.ErrorCount);
        }

        [Test]
        public void AddModelExceptionErrors_WhenExistingStateErrors_TheNewErrorsMessagesShouldBeAppended()
        {
            var exception = new CommitmentsApiModelException(_fixture.Add2ExceptionErrors().Errors);

            _fixture.Add1ModelStateErrors().ModelState.AddModelExceptionErrors(exception);

            Assert.AreEqual(3, _fixture.ModelState.ErrorCount);
        }

        [Test]
        public void AddModelExceptionErrors_WhenThereAreErrorsAndWeAreMappingfieldNames_TheFieldNamesWillBeMapped()
        {
            var exception = new CommitmentsApiModelException(_fixture.Add2ExceptionErrors().Errors);

            _fixture.ModelState.AddModelExceptionErrors(exception, field => "xxx" + field);

            Assert.IsTrue(_fixture.ModelState.ContainsKey("xxxfield1"));
            Assert.IsTrue(_fixture.ModelState.ContainsKey("xxxfield2"));
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
