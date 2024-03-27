using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.EditValidation;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships.EditValidation
{
    public class ValidateApprenticeshipForEditRequestToValidateApprenticeshipForEditCommandTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Maps_ValidateApprenticeshipForEditRequestToValidateApprenticeshipForEditCommand(
            ValidateApprenticeshipForEditRequest source,
            ValidateApprenticeshipForEditRequestToValidateApprenticeshipForEditCommand mapper)
        {
            var result = await mapper.Map(source);

            result.ApprenticeshipValidationRequest.ProviderId.Should().Be(source.ProviderId);
            result.ApprenticeshipValidationRequest.EmployerAccountId.Should().Be(source.EmployerAccountId);
            result.ApprenticeshipValidationRequest.ApprenticeshipId.Should().Be(source.ApprenticeshipId);
            result.ApprenticeshipValidationRequest.FirstName.Should().BeEquivalentTo(source.FirstName);
            result.ApprenticeshipValidationRequest.LastName.Should().BeEquivalentTo(source.LastName);
            result.ApprenticeshipValidationRequest.DateOfBirth.Should().Be(source.DateOfBirth);
            result.ApprenticeshipValidationRequest.ULN.Should().BeEquivalentTo(source.ULN);
            result.ApprenticeshipValidationRequest.Cost.Should().Be(source.Cost);
            result.ApprenticeshipValidationRequest.EmployerReference.Should().BeEquivalentTo(source.EmployerReference);
            result.ApprenticeshipValidationRequest.StartDate.Should().Be(source.StartDate);
            result.ApprenticeshipValidationRequest.EndDate.Should().Be(source.EndDate);
            result.ApprenticeshipValidationRequest.DeliveryModel.Should().Be(source.DeliveryModel);
            result.ApprenticeshipValidationRequest.CourseCode.Should().BeEquivalentTo(source.TrainingCode);
            result.ApprenticeshipValidationRequest.ProviderReference.Should().BeEquivalentTo(source.ProviderReference);
            result.ApprenticeshipValidationRequest.Email.Should().Be(source.Email);
            result.ApprenticeshipValidationRequest.EmploymentEndDate.Should().Be(source.EmploymentEndDate);
            result.ApprenticeshipValidationRequest.EmploymentPrice.Should().Be(source.EmploymentPrice);
        }
    }
}
