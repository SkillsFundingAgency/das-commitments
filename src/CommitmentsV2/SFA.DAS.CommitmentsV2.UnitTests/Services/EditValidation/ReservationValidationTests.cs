using SFA.DAS.CommitmentsV2.Types;
using System.IO;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class ReservationValidationTests
    {
        [Test]
        public async Task Reservation_Validations_Are_Returned()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship().SetupAuthenticationContextAsEmployer().SetupReservationValidationService();
            var request = fixture.CreateValidationRequest(employerRef: "abc", Party:Party.Employer);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Reason"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("CourseCode"));
            });
        }

        [Test]
        public async Task CorrectDateIsUsedForReservationValidation()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture
                .SetupMockContextApprenticeship()
                .SetupAuthenticationContextAsEmployer()
                .SetupReservationValidationService();

            var request = fixture.CreateValidationRequest(employerRef: "abc", Party: Party.Employer);
            await fixture.Validate(request);

            var expectedDate = fixture.Apprenticeship.StartDate.GetValueOrDefault();

            fixture.VerifyReservationValidationServiceIsCalledWithExpectedStartDate(expectedDate);
        }
    }
}
