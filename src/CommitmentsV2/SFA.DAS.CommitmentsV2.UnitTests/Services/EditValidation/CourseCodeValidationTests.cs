using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class CourseCodeValidationTests
    {
        [Test]
        public async Task CourseCode_Is_Mandatory()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var request = fixture.CreateValidationRequest(courseCode: string.Empty);

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Invalid training code", result.Errors[0].ErrorMessage);
            Assert.AreEqual("TrainingCode", result.Errors[0].PropertyName);
        }

        [Test]
        public async Task TransferSender_Funded_Can_Only_Have_Standard_Course()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship(transferSenderId : 2, startYear: 2022, endYear: 2029).SetUpMediatorForTrainingCourse(DateTime.Now, 1, Types.ProgrammeType.Framework);
            var request = fixture.CreateValidationRequest(courseCode: "5");

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Entered course is not valid.", result.Errors[0].ErrorMessage);
            Assert.AreEqual("TrainingCode", result.Errors[0].PropertyName);
        }
    }
}
