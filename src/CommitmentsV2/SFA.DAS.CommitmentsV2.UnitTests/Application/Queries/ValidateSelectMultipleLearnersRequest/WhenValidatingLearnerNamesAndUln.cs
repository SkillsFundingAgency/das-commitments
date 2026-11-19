using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.ValidateSelectMultipleLearnersRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload;
using UlnValidator = SFA.DAS.CommitmentsV2.Services.UlnValidator;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.ValidateSelectMultipleLearnersRequest;

[TestFixture]
public class WhenValidatingLearnerNamesAndUln
{
    [TestCase("wuqar<script>alert(1)</script>l853a", "Smith", "6591690157", "GivenName")]
    [TestCase("Mark", "blzm6<script>alert(1)</script>pghgc", "6591690157", "FamilyName")]
    [TestCase("Mark", "Smith", "kn93q<script>alert(1)</script>erxpk", "Uln")]
    [TestCase("<b>Mark</b>", "Smith", "6591690157", "GivenName")]
    [TestCase("Mark", "<b>Smith</b>", "6591690157", "FamilyName")]
    public async Task Then_Markup_Is_Returned_As_A_Field_Validation_Error(
        string firstName, string lastName, string uln, string expectedProperty)
    {
        using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
        fixture.SetGivenNames(firstName);
        fixture.SetFamilyName(lastName);
        fixture.SetUln(uln);
        var ulnValidator = new UlnValidator(new Learners.Validators.UlnValidator());
        fixture.MockUlnValidator.Setup(validator => validator.Validate(It.IsAny<string>()))
            .Returns<string>(ulnValidator.Validate);
        fixture.Db.Courses.Add(new Course
        {
            LarsCode = "59",
            Title = "Test course",
            Level = "2",
            EffectiveFrom = new DateTime(2000, 1, 1),
            EffectiveTo = new DateTime(2050, 1, 1)
        });
        fixture.Db.SaveChanges();
        var handler = new ValidateSelectMultipleLearnersQueryHandler(
            Mock.Of<ILogger<ValidateSelectMultipleLearnersQueryHandler>>(),
            new Lazy<ProviderCommitmentsDbContext>(() => fixture.Db),
            fixture.EmployerAgreementService.Object,
            Mock.Of<ITrainingProgrammeLookup>(),
            fixture.RplSettingsConfig,
            fixture.ValidationService);
        var query = new ValidateSelectMultipleLearnersQuery
        {
            ProviderId = BulkUploadValidateCommandHandlerTestsFixture.ProviderId,
            CsvRecords = fixture.CsvRecords,
            ProviderStandardResults = fixture.Command.ProviderStandardResults,
            ReservationValidationResults = fixture.Command.ReservationValidationResults,
            OtjTrainingHours = fixture.Command.OtjTrainingHours
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ValidationErrors.Should().ContainSingle()
            .Which.Errors.Should().ContainSingle(error => error.Property == expectedProperty);
    }
}
