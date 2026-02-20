using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.LinkGeneration;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload;

public class BulkUploadValidateCommandHandlerTestsFixture : IDisposable
{
    public ProviderCommitmentsDbContext Db { get; set; }
    public IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse> Handler { get; set; }
    public Mock<IOverlapCheckService> OverlapCheckService { get; set; }
    public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; set; }
    public Mock<IEmployerAgreementService> EmployerAgreementService { get; set; }
    public List<BulkUploadAddDraftApprenticeshipRequest> CsvRecords { get; set; }
    public BulkUploadValidateCommand Command { get; set; }
    public Mock<IProviderRelationshipsApiClient> ProviderRelationshipsApiClient { get; set; }
    public OverlapCheckResult OverlapCheckResult { get; set; }
    public EmailOverlapCheckResult EmailOverlapCheckResult { get; set; }
    public RplSettingsConfiguration RplSettingsConfig { get; set; }
    public bool IsAgreementSigned { get; set; } = true;
    public DraftApprenticeship DraftApprenticeship { get; private set; }
    public Cohort Cohort { get; set; }
    public Mock<IUlnValidator> MockUlnValidator;
    protected Mock<ILinkGenerator> MockLinkGenerator;
    public const long ProviderId = 333;

    public BulkUploadValidateCommandHandlerTestsFixture()
    {
        MockLinkGenerator = new Mock<ILinkGenerator>();

        MockUlnValidator = new Mock<IUlnValidator>();
        MockUlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.Success);

        CsvRecords = [];
        PopulateCsvRecord();
        Command = new BulkUploadValidateCommand
        {
            CsvRecords = CsvRecords,
            ProviderId = ProviderId,
            ProviderStandardResults =
            {
                Standards = new List<ProviderStandard> { new("123", "123") }
            },
            OtjTrainingHours = new Dictionary<string, int?>()
        };

        OverlapCheckService = new Mock<IOverlapCheckService>();
        OverlapCheckResult = new OverlapCheckResult(false, false);

        OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, CancellationToken.None))
            .ReturnsAsync(() => OverlapCheckResult);

        var listUlnOverlap = new List<OverlapCheckResult>
        {
            new(false, false)
        };
        OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => listUlnOverlap);

        EmailOverlapCheckResult = new EmailOverlapCheckResult(1, OverlapStatus.None, false);
        OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, null, CancellationToken.None))
            .ReturnsAsync(() => EmailOverlapCheckResult);

        var listEmailOverlap = new List<EmailOverlapCheckResult>
        {
            new(1, OverlapStatus.None, true)
        };
        OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listEmailOverlap);

        AcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
        AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(DateTime.Parse(CsvRecords[0].StartDateAsString));

        EmployerAgreementService = new Mock<IEmployerAgreementService>();
        EmployerAgreementService.Setup(x => x.IsAgreementSigned(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(() => IsAgreementSigned);

        Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);
        SetupDbData();

        ProviderRelationshipsApiClient = new Mock<IProviderRelationshipsApiClient>();
        ProviderRelationshipsApiClient.Setup(x => x.HasPermission(It.IsAny<HasPermissionRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => true);
        RplSettingsConfig = new RplSettingsConfiguration { MinimumPriceReduction = 100, MaximumTrainingTimeReduction = 999 };

        Handler = new BulkUploadValidateCommandHandler(Mock.Of<ILogger<BulkUploadValidateCommandHandler>>()
            , new Lazy<ProviderCommitmentsDbContext>(() => Db)
            , OverlapCheckService.Object
            , AcademicYearDateProvider.Object
            , ProviderRelationshipsApiClient.Object
            , EmployerAgreementService.Object
            , RplSettingsConfig
            , MockUlnValidator.Object
            , MockLinkGenerator.Object
        );
    }

    private void SetupDbData()
    {
        var fixture = new Fixture();
        var account = new Account()
            .Set(a => a.Name, "Employer1")
            .Set(a => a.LevyStatus, ApprenticeshipEmployerType.Levy);

        var ale = new AccountLegalEntity()
            .Set(al => al.PublicHashedId, "XEGE5X")
            .Set(al => al.Account, account);

        Cohort = new Cohort()
            .Set(c => c.Id, 111)
            .Set(c => c.EmployerAccountId, 222)
            .Set(c => c.ProviderId, ProviderId)
            .Set(c => c.Reference, "P97BKL")
            .Set(c => c.WithParty, Party.Provider)
            .Set(c => c.AccountLegalEntity, ale);

        var standard = new Standard()
            .Set(x => x.LarsCode, 59)
            .Set(x => x.StandardUId, Guid.NewGuid().ToString())
            .Set(x => x.EffectiveFrom, new DateTime(2000, 1, 1))
            .Set(x => x.EffectiveTo, new DateTime(2050, 1, 1));

        var draftApprenticeship = new DraftApprenticeship()
            .Set(d => d.Id, 100)
            .Set(d => d.FirstName, "James")
            .Set(d => d.LastName, "Opus")
            .Set(d => d.DateOfBirth, new DateTime(2001, 05, 09))
            .Set(d => d.Cost, 1000)
            .Set(d => d.Uln, "6591690154")
            .Set(d => d.Email, "abc09@test.com")
            .Set(d => d.StartDate, new DateTime(2021, 10, 1))
            .Set(d => d.EndDate, new DateTime(2022, 10, 1))
            .Set(d => d.CourseName, "coursename");
        Cohort.Apprenticeships.Add(draftApprenticeship);

        Db.Cohorts.Add(Cohort);
        Db.Standards.Add(standard);
        Db.SaveChanges();
    }

    internal async Task<Standard> GetStandard()
    {
        var standard = await Db.Standards.FirstOrDefaultAsync();
        return standard;
    }

    private void PopulateCsvRecord()
    {
        CsvRecords.Add(new BulkUploadAddDraftApprenticeshipRequest
        {
            RowNumber = 1,
            AgreementId = "XEGE5X",
            CohortRef = "P97BKL",
            Uln = "6591690157",
            LastName = "Smith",
            FirstName = "Mark",
            DateOfBirthAsString = "2000-01-02",
            CourseCode = "59",
            StartDateAsString = "2023-05-01",
            EndDateAsString = "2024-05",
            CostAsString = "2000",
            ProviderRef = "ZB88",
            Email = "abc34628125987@abc.com",
            RecognisePriorLearningAsString = "false",
            MinimumAgeAtApprenticeshipStart = 15,
            MaximumAgeAtApprenticeshipStart = 115
        });
    }

    internal void SetUpDuplicateUln()
    {
        CsvRecords.Add(new BulkUploadAddDraftApprenticeshipRequest
        {
            RowNumber = 2,
            AgreementId = "XEGE5X",
            CohortRef = "P97BKL",
            Uln = "6591690157",
            LastName = "Smith2",
            FirstName = "Mark2",
            DateOfBirthAsString = "2002-01-02",
            CourseCode = "59",
            StartDateAsString = "2019-05-01",
            EndDateAsString = "2020-05",
            CostAsString = "2000",
            ProviderRef = "ZB88",
            Email = "abc34628125987@abc2.com",
            MinimumAgeAtApprenticeshipStart = 15,
            MaximumAgeAtApprenticeshipStart = 115

        });
    }

    internal void SetUpDuplicateEmail()
    {
        CsvRecords.Add(new BulkUploadAddDraftApprenticeshipRequest
        {
            RowNumber = 2,
            AgreementId = "XEGE5X",
            CohortRef = "P97BKL",
            Uln = "6591690168",
            LastName = "Smith2",
            FirstName = "Mark2",
            DateOfBirthAsString = "2002-01-02",
            CourseCode = "59",
            StartDateAsString = "2019-05-01",
            EndDateAsString = "2020-05",
            CostAsString = "2000",
            ProviderRef = "ZB88",
            Email = "abc34628125987@abc.com",
            MinimumAgeAtApprenticeshipStart = 15,
            MaximumAgeAtApprenticeshipStart = 115
        });
    }

    internal void SetUpDuplicateUlnWithinTheSameCohort()
    {
        CsvRecords.Add(new BulkUploadAddDraftApprenticeshipRequest
        {
            RowNumber = 3,
            AgreementId = "XEGE5X",
            CohortRef = "P97BKL",
            Uln = "6591690158",
            LastName = "Smith3",
            FirstName = "Mark3",
            DateOfBirthAsString = "2002-01-02",
            CourseCode = "59",
            StartDateAsString = "2019-05-01",
            EndDateAsString = "2020-05",
            CostAsString = "2000",
            ProviderRef = "ZB88",
            Email = "abc34628125987@abc3.com",
            MinimumAgeAtApprenticeshipStart = 15,
            MaximumAgeAtApprenticeshipStart = 115

        });

        Cohort.Apprenticeships.Add(new DraftApprenticeship
        {
            Id = 101,
            FirstName = "Ganga",
            LastName = "John",
            DateOfBirth = new DateTime(2002, 05, 09),
            Cost = 1500,
            Uln = "6591690158",
            Email = "abc@test.com",
            StartDate = new DateTime(2021, 10, 1),
            EndDate = new DateTime(2022, 10, 1),
            CourseName = "coursename"
        });
    }

    internal void SetUpDuplicateEmailWithinTheSameCohort()
    {
        CsvRecords.Add(new BulkUploadAddDraftApprenticeshipRequest
        {
            RowNumber = 4,
            AgreementId = "XEGE5X",
            CohortRef = "P97BKL",
            Uln = "6591690178",
            LastName = "Smith4",
            FirstName = "Mark4",
            DateOfBirthAsString = "2002-01-02",
            CourseCode = "59",
            StartDateAsString = "2019-05-01",
            EndDateAsString = "2020-05",
            CostAsString = "2000",
            ProviderRef = "ZB88",
            Email = "abc@test.com",
            MinimumAgeAtApprenticeshipStart = 15,
            MaximumAgeAtApprenticeshipStart = 115
        });

        Cohort.Apprenticeships.Add(new DraftApprenticeship
        {
            Id = 101,
            FirstName = "Patricia",
            LastName = "John",
            DateOfBirth = new DateTime(2002, 05, 09),
            Cost = 1500,
            Uln = "6591690158",
            Email = "abc@test.com",
            StartDate = new DateTime(2021, 10, 1),
            EndDate = new DateTime(2022, 10, 1),
            CourseName = "coursename"
        });
    }

    public Task<BulkUploadValidateApiResponse> Handle()
    {
        return Handler.Handle(Command, CancellationToken.None);
    }

    public static void ValidateError(BulkUploadValidateApiResponse errors, int numberOfErrors, string property, string errorText)
    {
        Assert.Multiple(() =>
        {
            Assert.That(errors.BulkUploadValidationErrors, Has.Count.EqualTo(numberOfErrors));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors, Has.Count.EqualTo(numberOfErrors));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].ErrorText, Is.EqualTo(errorText));
            Assert.That(errors.BulkUploadValidationErrors[0].Errors[0].Property, Is.EqualTo(property));
        });
    }

    public static void ValidateError(BulkUploadValidateApiResponse errors, string property, string errorText)
    {
        errors.Should().NotBeNull();
        errors.BulkUploadValidationErrors.Should().NotBeEmpty();
        errors.BulkUploadValidationErrors[0].Errors.Should().ContainEquivalentOf(new
        {
            Property = property,
            ErrorText = errorText,
        });
    }

    public static void ValidateNoErrorsFound(BulkUploadValidateApiResponse errors)
    {
        Assert.That(errors.BulkUploadValidationErrors, Is.Empty);
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetCohortRef(string cohortRef)
    {
        CsvRecords[0].CohortRef = cohortRef;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetAgreementId(string agreementId)
    {
        CsvRecords[0].AgreementId = agreementId;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetProviderRef(string providerRef)
    {
        CsvRecords[0].ProviderRef = providerRef;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetStartDate(string startDate)
    {
        CsvRecords[0].StartDateAsString = startDate;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetEndDate(string endDate)
    {
        CsvRecords[0].EndDateAsString = endDate;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetAfterAcademicYearEndDate()
    {
        AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(DateTime.Parse(CsvRecords[0].StartDateAsString).AddYears(-1).AddDays(-1));
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetProviderHasPermissionToCreateCohort(bool hasPermission)
    {
        ProviderRelationshipsApiClient.Setup(x => x.HasPermission(It.IsAny<HasPermissionRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => hasPermission);
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetCourseEffectiveFromAfterCourseStartDate()
    {
        var standard = Db.Standards.FirstOrDefaultAsync().Result;
        standard.EffectiveFrom = DateTime.Parse(CsvRecords[0].StartDateAsString).AddDays(1);
        Db.SaveChanges();
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetCourseEffectiveToBeforeCourseStartDate()
    {
        var standard = Db.Standards.FirstOrDefaultAsync().Result;
        standard.EffectiveTo = DateTime.Parse(CsvRecords[0].StartDateAsString).AddDays(-1);
        Db.SaveChanges();
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetIsTransferFunded()
    {
        var cohort = Db.Cohorts.FirstOrDefaultAsync().Result;
        cohort.TransferSenderId = 1;
        Db.SaveChanges();
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetLevyStatus(ApprenticeshipEmployerType apprenticeshipEmployerType)
    {
        var account = Db.Accounts.FirstOrDefaultAsync().Result;
        account.LevyStatus = apprenticeshipEmployerType;
        Db.SaveChanges();
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetStdCode(string stdCode)
    {
        CsvRecords[0].CourseCode = stdCode;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetMainProvider(bool isMainProvider)
    {
        Command.ProviderStandardResults.IsMainProvider = isMainProvider;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetStandardsEmpty()
    {
        Command.ProviderStandardResults.Standards = [];

        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetStandards(string courseCode, string name, int? level)
    {
        Command.ProviderStandardResults.Standards = [new ProviderStandard(courseCode,name, level)];

        if (int.TryParse(courseCode, out var larsCode) && level.HasValue)
        {
            var dbStandard = Db.Standards.FirstOrDefault(x => x.LarsCode == larsCode);
            if (dbStandard != null)
            {
                dbStandard.Level = level.Value;
                Db.SaveChanges();
            }
        }

        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetGivenNames(string givenName)
    {
        CsvRecords[0].FirstName = givenName;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetFamilyName(string familyName)
    {
        CsvRecords[0].LastName = familyName;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetEmailAddress(string emailAddress)
    {
        CsvRecords[0].Email = emailAddress;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetDateOfBirth(string dateOfBirth)
    {
        CsvRecords[0].DateOfBirthAsString = dateOfBirth;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetMinAge(int? minAge)
    {
        CsvRecords[0].MinimumAgeAtApprenticeshipStart = minAge;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetMaxAge(int? maxAge)
    {
        CsvRecords[0].MaximumAgeAtApprenticeshipStart = maxAge;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetUln(string uln)
    {
        CsvRecords[0].Uln = uln;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetTotalPrice(string totalPrice)
    {
        CsvRecords[0].CostAsString = totalPrice;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetWithParty(Party withParty)
    {
        Db.Cohorts.FirstOrDefaultAsync().Result.WithParty = withParty;
        Db.SaveChanges();
        return this;
    }

    internal void SetOverlappingDate(bool startDate, bool endDate)
    {
        OverlapCheckResult = new OverlapCheckResult(startDate, endDate);
    }

    internal void SetOverlappingEmail(OverlapStatus status)
    {
        EmailOverlapCheckResult = new EmailOverlapCheckResult(1, status, true);
    }

    internal void SetUpOverlappingUlnWithinTheSameCohort(bool startDate, bool endDate)
    {
        var listUlnOverlap = new List<OverlapCheckResult>
        {
            new(startDate, endDate)
        };
        OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => listUlnOverlap);
    }

    internal void SetOverlappingEmailWithinTheSameCohort(OverlapStatus status)
    {
        var listEmailOverlap = new List<EmailOverlapCheckResult>
        {
            new(1, status, true)
        };
        OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listEmailOverlap);
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetRecognisePriorLearning(string recognisePriorLearning)
    {
        CsvRecords[0].RecognisePriorLearningAsString = recognisePriorLearning;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetTrainingTotalHours(string trainingTotalHours)
    {
        CsvRecords[0].TrainingTotalHoursAsString = trainingTotalHours;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetTrainingHoursReduction(string trainingHoursReduction)
    {
        CsvRecords[0].TrainingHoursReductionAsString = trainingHoursReduction;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetIsDurationReducedByRpl(string isDurationReducedByRpl)
    {
        CsvRecords[0].IsDurationReducedByRPLAsString = isDurationReducedByRpl;
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetDurationReducedBy(string durationReducedBy)
    {
        CsvRecords[0].DurationReducedByAsString = durationReducedBy;
        return this;
    }

    internal void SetPriorLearning(bool? recognisePriorLearning, int? durationReducedBy = null, int? priceReducedBy = null, int? trainingTotalHours = null, int? trainingHoursReduction = null, bool? isDurationReducedByRpl = null)
    {
        CsvRecords[0].RecognisePriorLearningAsString = recognisePriorLearning?.ToString();
        CsvRecords[0].DurationReducedByAsString = durationReducedBy.ToString();
        CsvRecords[0].PriceReducedByAsString = priceReducedBy.ToString();
        CsvRecords[0].TrainingTotalHoursAsString = trainingTotalHours.ToString();
        CsvRecords[0].TrainingHoursReductionAsString = trainingHoursReduction.ToString();
        CsvRecords[0].IsDurationReducedByRPLAsString = isDurationReducedByRpl.ToString();
    }

    internal void SetPriorLearningRaw(bool? recognisePriorLearning, string durationReducedByAsString = null, string priceReducedByAsString = null, string trainingTotalHoursAsString = null,
        string trainingHoursReductionAsString = null, string isDurationReducedByRplAsString = null)
    {
        CsvRecords[0].RecognisePriorLearningAsString = recognisePriorLearning?.ToString();
        CsvRecords[0].DurationReducedByAsString = durationReducedByAsString;
        CsvRecords[0].PriceReducedByAsString = priceReducedByAsString;
        CsvRecords[0].TrainingTotalHoursAsString = trainingTotalHoursAsString;
        CsvRecords[0].TrainingHoursReductionAsString = trainingHoursReductionAsString;
        CsvRecords[0].IsDurationReducedByRPLAsString = isDurationReducedByRplAsString;
    }

    internal void SetUpIncompleteRecord()
    {
        DraftApprenticeship = new DraftApprenticeship
        {
            Id = 100,
            FirstName = "James",
            Cost = 1000,
            Uln = "6591690154",
            Email = "abc09@test.com",
            StartDate = new DateTime(2021, 10, 1),
            EndDate = new DateTime(2022, 10, 1),
            CourseName = "coursename"
        };

        Cohort.Apprenticeships.Add(DraftApprenticeship);
    }


    internal void SetIsAgreementSigned(bool isAgreementSigned)
    {
        IsAgreementSigned = isAgreementSigned;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetChangeOfParty()
    {
        var account = new Account()
            .Set(a => a.Id, 20)
            .Set(a => a.Name, "Employer2")
            .Set(a => a.LevyStatus, ApprenticeshipEmployerType.Levy);

        var ale = new AccountLegalEntity()
            .Set(a => a.Id, 20)
            .Set(al => al.PublicHashedId, "XEGE5Y")
            .Set(al => al.Account, account);

        Db.Accounts.Add(account);
        Db.SaveChanges();

        var changeOfPartyCohort = new Cohort()
            .Set(a => a.Id, 20)
            .Set(x => x.ProviderId, 999)
            .Set(c => c.Reference, "P97BKM")
            .Set(c => c.WithParty, Party.Provider)
            .Set(c => c.AccountLegalEntity, ale);

        Db.Cohorts.Add(changeOfPartyCohort);
        Db.SaveChanges();

        var fixture = new Fixture();
        var cohort = Db.Cohorts.FirstOrDefaultAsync().Result;
        var apprenticeship = new Apprenticeship();

        apprenticeship.Set(x => x.FirstName, "FirstName");
        apprenticeship.Set(x => x.LastName, "LastName");
        apprenticeship.Set(x => x.Email, "abc@hotmail.com");
        apprenticeship.Set(x => x.EmailAddressConfirmed, true);
        apprenticeship.Set(x => x.Cohort, changeOfPartyCohort);

        Db.Apprenticeships.Add(apprenticeship);

        var request = new ChangeOfPartyRequest();

        request.Set(x => x.Apprenticeship, apprenticeship);
        request.Set(x => x.ChangeOfPartyType, fixture.Create<ChangeOfPartyRequestType>());
        request.Set(x => x.OriginatingParty, fixture.Create<Party>());
        request.Set(x => x.Status, fixture.Create<ChangeOfPartyRequestStatus>());
        request.Set(x => x.AccountLegalEntity, cohort.AccountLegalEntity);
        request.Set(x => x.StartDate, fixture.Create<DateTime>());
        request.Set(x => x.EndDate, fixture.Create<DateTime>());

        cohort.ChangeOfPartyRequest = request;
        cohort.ChangeOfPartyRequestId = request.Id;
        Db.SaveChanges();
        return this;
    }

    internal BulkUploadValidateCommandHandlerTestsFixture SetEpaOrgId(string epaOrgId)
    {
        CsvRecords[0].EPAOrgId = epaOrgId;
        return this;
    }

    public void Dispose()
    {
        Db?.Dispose();
        GC.SuppressFinalize(this);
    }
}