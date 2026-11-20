using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.ValidateSelectMultipleLearnersRequest;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.LinkGeneration;
using SFA.DAS.CommitmentsV2.Models;
using Course = SFA.DAS.CommitmentsV2.Models.Course;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;
using SFA.DAS.CommitmentsV2.Services.ValidationService;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.ValidateSelectMultipleLearnersRequest;

[TestFixture]
public class WhenValidatingAStartDateAgainstTheCalculatedStandardVersion
{
    private ProviderCommitmentsDbContext _db;
    private Mock<ITrainingProgrammeLookup> _trainingProgrammeLookup;
    private StartDateCapturingValidationService _validationService;
    private ValidateSelectMultipleLearnersQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        _db.Courses.Add(new Course
        {
            LarsCode = "192",
            Title = "Building services engineering ductwork installer",
            Level = "2",
            EffectiveFrom = new DateTime(2017, 9, 12),
            EffectiveTo = null
        });
        _db.SaveChanges();

        var academicYear = new Mock<IAcademicYearDateProvider>();
        academicYear.Setup(x => x.CurrentAcademicYearEndDate).Returns(new DateTime(2030, 7, 31));

        var startDateValidation = new ValidationService(
            Mock.Of<ILogger<ValidationService>>(),
            Mock.Of<IOverlapCheckService>(),
            academicYear.Object,
            Mock.Of<IProviderRelationshipsApiClient>(),
            Mock.Of<IEmployerAgreementService>(),
            Mock.Of<IUlnValidator>(),
            Mock.Of<ILinkGenerator>());

        _trainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
        _validationService = new StartDateCapturingValidationService(startDateValidation);
        _handler = new ValidateSelectMultipleLearnersQueryHandler(
            Mock.Of<ILogger<ValidateSelectMultipleLearnersQueryHandler>>(),
            new Lazy<ProviderCommitmentsDbContext>(() => _db),
            Mock.Of<IEmployerAgreementService>(),
            _trainingProgrammeLookup.Object,
            new RplSettingsConfiguration(),
            _validationService);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task Then_a_start_after_the_standard_effective_to_is_returned_as_a_validation_error()
    {
        var effectiveTo = new DateTime(2023, 7, 26);
        _trainingProgrammeLookup
            .Setup(x => x.GetCalculatedTrainingProgrammeVersion("192", new DateTime(2026, 10, 1)))
            .ReturnsAsync(new TrainingProgramme(
                "192",
                "Building services engineering ductwork installer",
                ProgrammeType.Standard,
                new DateTime(2017, 9, 12),
                effectiveTo));

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        _validationService.StandardPassedToStartDate.EffectiveTo.Should().Be(effectiveTo);
        var error = result.ValidationErrors.Should().ContainSingle().Subject;
        error.Errors.Should().ContainSingle();
        error.Errors.Single().Property.Should().Be("StartDate");
        error.Errors.Single().ErrorText.Should().Be(
            "This training course is only available to apprentices with a <b>start date</b> before 8  2023");
    }

    [Test]
    public async Task Then_course_dates_are_kept_when_no_standard_version_is_calculated()
    {
        _trainingProgrammeLookup
            .Setup(x => x.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync((TrainingProgramme)null);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        _validationService.StandardPassedToStartDate.EffectiveTo.Should().BeNull();
        result.ValidationErrors.Should().BeEmpty();
    }

    private static ValidateSelectMultipleLearnersQuery CreateQuery()
    {
        return new ValidateSelectMultipleLearnersQuery
        {
            ProviderId = 10005760,
            CsvRecords =
            [
                new BulkUploadAddDraftApprenticeshipRequest
                {
                    CourseCode = "192",
                    StartDateAsString = "2026-10-05",
                    Uln = "5056657319",
                    FirstName = "Molly",
                    LastName = "Parsons"
                }
            ]
        };
    }

    private sealed class StartDateCapturingValidationService(IValidationService startDateValidation) : IValidationService
    {
        public Standard StandardPassedToStartDate { get; private set; }

        public IEnumerable<Error> ValidateStartDate(BulkUploadAddDraftApprenticeshipRequest csvRecord, Standard standard, Cohort cohort)
        {
            StandardPassedToStartDate = standard;
            return startDateValidation.ValidateStartDate(csvRecord, standard, cohort);
        }

        public Task<List<Error>> ValidateAgreementIdValidFormat(string agreementId, string employerName) => Task.FromResult(new List<Error>());

        public Task<List<Error>> ValidateAgreementIdIsSigned(bool? isSigned) => Task.FromResult(new List<Error>());

        public Task<List<Error>> ValidateCohortRef(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, Cohort cohort, string employerName) => Task.FromResult(new List<Error>());

        public Task<bool> ValidatePermissionToCreateCohort(long providerId, ICollection<Error> domainErrors, bool? isLevy, EmployerSummary employerDetails) => Task.FromResult(true);

        public IEnumerable<Error> ValidateCost(string costAsString, int? cost) => [];

        public IEnumerable<Error> ValidateDateOfBirth(BulkUploadAddDraftApprenticeshipRequest csvRecord, ProviderStandardResults providerStandardResults, Standard standard) => [];

        public IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord) => [];

        public IEnumerable<Error> ValidateEmailAddress(BulkUploadAddDraftApprenticeshipRequest csvRecord, List<BulkUploadAddDraftApprenticeshipRequest> csvRecords) => [];

        public IEnumerable<Error> ValidateEndDate(BulkUploadAddDraftApprenticeshipRequest csvRecord) => [];

        public IEnumerable<Error> ValidateEPAOrgId(string EPAOrgId) => [];

        public IEnumerable<Error> ValidateLastName(string lastName) => [];

        public IEnumerable<Error> ValidateFirstName(string firstName) => [];

        public IEnumerable<Error> ValidatePriceReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minPriceReduction) => [];

        public IEnumerable<Error> ValidateProviderRef(string providerRef) => [];

        public IEnumerable<Error> ValidateRecognisePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord) => [];

        public IEnumerable<Error> ValidateReservation(BulkUploadAddDraftApprenticeshipRequest csvRecord, BulkReservationValidationResults reservationValidationResults) => [];

        public IEnumerable<Error> ValidateTrainingHoursReduction(BulkUploadAddDraftApprenticeshipRequest csvRecord, int maxTrainingHoursReduction, int minimumOffTheJobTrainingHoursForCourse) => [];

        public IEnumerable<Error> ValidateTrainingTotalHours(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minimumOffTheJobTrainingHoursForCourse) => [];

        public IEnumerable<Error> ValidateUln(BulkUploadAddDraftApprenticeshipRequest csvRecord, List<BulkUploadAddDraftApprenticeshipRequest> csvRecords) => [];

        public List<Error> ValidateDeclaredStandards(ProviderStandardResults providerStandardResults) => [];

        public IEnumerable<Error> ValidateCourseCode(long providerId, string courseCode, ProviderStandardResults providerStandardResults, Standard standard, bool allowAppUnits = false) => [];
    }
}
