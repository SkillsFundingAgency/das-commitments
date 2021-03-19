using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships.EditValidation;
using SFA.DAS.Testing.AutoFixture;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships.EditValidation
{
    class ValidateApprenticeshipForEditCommandToValidateApprenticeshipRequestMapperTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Maps_ValidateApprenticeshipForEditCommandToValidateApprenticeshipRequestMapperTests(
            ValidateApprenticeshipForEditCommand source,
            ValidateApprenticeshipForEditCommandToValidateApprenticeshipRequestMapper mapper)
        {
            var result = await mapper.Map(source);

            result.ProviderId.Should().Be(source.ProviderId);
            result.EmployerAccountId.Should().Be(source.EmployerAccountId);
            result.ApprenticeshipId.Should().Be(source.ApprenticeshipId);
            result.FirstName.Should().BeEquivalentTo(source.FirstName);
            result.LastName.Should().BeEquivalentTo(source.LastName);
            result.DateOfBirth.Should().Be(source.DateOfBirth);
            result.ULN.Should().BeEquivalentTo(source.ULN);
            result.Cost.Should().Be(source.Cost);
            result.EmployerReference.Should().BeEquivalentTo(source.EmployerReference);
            result.StartDate.Should().Be(source.StartDate);
            result.EndDate.Should().Be(source.EndDate);
            result.TrainingCode.Should().BeEquivalentTo(source.TrainingCode);
        }
    }
}
