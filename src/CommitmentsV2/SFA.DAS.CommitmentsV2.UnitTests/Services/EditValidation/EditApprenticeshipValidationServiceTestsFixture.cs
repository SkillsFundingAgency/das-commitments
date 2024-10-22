using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using DateRange = SFA.DAS.CommitmentsV2.Domain.Entities.DateRange;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Types.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation;

public class EditApprenticeshipValidationServiceTestsFixture
{
    private readonly EditApprenticeshipValidationService _sut;
    private readonly Mock<IProviderCommitmentsDbContext> _dbContext;
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IOverlapCheckService> _overlapCheckService;
    private readonly Mock<IReservationValidationService> _reservationValidationService;
    private readonly Mock<IAcademicYearDateProvider> _academicYearDateProvider;
    private readonly Mock<ICurrentDateTime> _currentDateTime;
    private readonly Mock<IAuthenticationService> _authenticationService;

    public DateTime? StartDate => Apprenticeship.StartDate;

    internal DateTime GetEndOfCurrentTeachingYear()
    {
        return _academicYearDateProvider.Object.CurrentAcademicYearEndDate;
    }

    public Apprenticeship Apprenticeship { get; private set; }

    public EditApprenticeshipValidationServiceTestsFixture()
    {
        _dbContext = new Mock<IProviderCommitmentsDbContext>();
        _mediator = new Mock<IMediator>();
        _overlapCheckService = new Mock<IOverlapCheckService>();
        _reservationValidationService = new Mock<IReservationValidationService>();
        _academicYearDateProvider = new Mock<IAcademicYearDateProvider>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _authenticationService = new Mock<IAuthenticationService>();

        _sut = new EditApprenticeshipValidationService(_dbContext.Object, _mediator.Object,
            _overlapCheckService.Object,
            _reservationValidationService.Object,
            _academicYearDateProvider.Object,
            _currentDateTime.Object,
            _authenticationService.Object);

        _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long>(), CancellationToken.None))
            .Returns(Task.FromResult(new OverlapCheckResult(false, false)));

        _currentDateTime.Setup(x => x.UtcNow).Returns(() => new DateTime(2021, 3, 19));

        _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None))
            .Returns(Task.FromResult(new ReservationValidationResult(Array.Empty<ReservationValidationError>())));
    }

    internal DateTime GetCurrentAcademicYearStartDate()
    {
        return _academicYearDateProvider.Object.CurrentAcademicYearStartDate;
    }

    internal void SetUpLastAcademicYearFundingPeriodToBeBeforeDateTimeNow()
    {
        _academicYearDateProvider.Setup(t => t.LastAcademicYearFundingPeriod).Returns(_currentDateTime.Object.UtcNow.AddMonths(-1));
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupReservationValidationService()
    {
        _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None))
            .Returns(Task.FromResult(new ReservationValidationResult(new ReservationValidationError[1] {
                new("CourseCode","Reason")
            })));
        return this;
    }

    internal EditApprenticeshipValidationServiceTestsFixture CourseIsEffectiveFromDate(DateTime effectiveFrom, int activeForInYears = 5, ProgrammeType programmeType = ProgrammeType.Standard)
    {
        _mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeQuery>(), CancellationToken.None))
            .Returns(Task.FromResult(new GetTrainingProgrammeQueryResult
            {
                TrainingProgramme = new TrainingProgramme
                {
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = effectiveFrom.AddYears(activeForInYears),
                    ProgrammeType = programmeType
                }
            }));

        return this;
    }

    internal DateTime GetStartDate()
    {
        return Apprenticeship.StartDate.Value;
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupOverlapService(bool startDateOverlaps, bool endDateOverlaps)
    {
        _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long>(), CancellationToken.None))
            .Returns(Task.FromResult(new OverlapCheckResult(startDateOverlaps, endDateOverlaps)));
        return this;
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupGetTrainingProgrammeQueryResult()
    {
        var result = new GetTrainingProgrammeQueryResult { TrainingProgramme = new TrainingProgramme { EffectiveFrom = new DateTime(2017, 04, 01), EffectiveTo = new DateTime(2030, 04, 01) } };
        _mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        return this;
    }

    private void WithStartDateInFuture()
    {
        Apprenticeship.ActualStartDate = _currentDateTime.Object.UtcNow.AddMonths(1);
        Apprenticeship.StartDate = new DateTime(Apprenticeship.ActualStartDate.Value.Year, Apprenticeship.ActualStartDate.Value.Month, 1);
        Apprenticeship.EndDate = _currentDateTime.Object.UtcNow.AddYears(1);
    }

    private EditApprenticeshipValidationServiceTestsFixture WithInFundingPeriod()
    {
        _academicYearDateProvider.Setup(t => t.CurrentAcademicYearStartDate).Returns(Apprenticeship.StartDate.Value.AddMonths(-1));

        _academicYearDateProvider.Setup(t => t.CurrentAcademicYearEndDate).Returns(Apprenticeship.StartDate.Value.AddYears(1));

        _academicYearDateProvider.Setup(t => t.LastAcademicYearFundingPeriod).Returns(_currentDateTime.Object.UtcNow.AddMonths(2));

        return this;
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupMockContextApprenticeship(
        long id = 100,
        long commitmentId = 200,
        string firstName = "FirstName",
        string lastName = "lastName",
        string email = null,
        int dobYear = 1995,
        int dobMonth = 1,
        int dobDay = 1,
        string employerRef = "employerRef",
        string uln = "XYZ123",
        string courseCode = "12",
        ProgrammeType programmeType = ProgrammeType.Standard,
        int? transferSenderId = null,
        decimal cost = 200,
        string reservationId = "134463EF-0088-4828-8775-EBD1223486AF",
        PaymentStatus paymentStatus = PaymentStatus.Active,
        bool hasHadDataLockSuccess = false,
        DateTime employerProviderApprovedOn = default,
        DeliveryModel deliveryModel = DeliveryModel.Regular,
        FlexibleEmployment flexibleEmployment = null,
        bool isOnFlexiPaymentsPilot = false)
            
    {
        CreateApprenticeship(id, commitmentId, firstName, lastName, email, dobYear, dobMonth, dobDay, employerRef, uln, courseCode, programmeType, transferSenderId, cost, 
            reservationId, paymentStatus, hasHadDataLockSuccess, employerProviderApprovedOn, deliveryModel, flexibleEmployment, isOnFlexiPaymentsPilot);

        WithStartDateInFuture();

        WithInFundingPeriod();

        WithPriceHistoryWithStartDate(cost);

        CreateMockApprenticeshipContext();
        return this;
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupOverlapCheckServiceToReturnEmailOverlap(string email)
    {
        var result = new EmailOverlapCheckResult(1, OverlapStatus.DateWithin, true);
        _overlapCheckService.Setup(x =>
            x.CheckForEmailOverlaps(email, It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long?>(),
                It.IsAny<long?>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        return this;
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupAuthenticationContextAsEmployer()
    {
        _authenticationService.Setup(x => x.GetUserParty()).Returns(Party.Employer);
        return this;
    }

    public EditApprenticeshipValidationServiceTestsFixture SetupAuthenticationContextAsProvider()
    {
        _authenticationService.Setup(x => x.GetUserParty()).Returns(Party.Provider);
        return this;
    }

    public void VerifyCheckForEmailOverlapsIsNotCalled()
    {
        var result = new EmailOverlapCheckResult(1, OverlapStatus.DateWithin, true);
        _overlapCheckService.Verify(x =>
            x.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long?>(),
                It.IsAny<long?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void VerifyCheckForEmailOverlapsIsCalledWithExpectedStartDate(DateTime startDate)
    {
        _overlapCheckService.Verify(x =>
            x.CheckForEmailOverlaps(It.IsAny<string>(), It.Is<DateRange>(dr => dr.From == startDate), It.IsAny<long?>(),
                It.IsAny<long?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void VerifyCheckForOverlapsIsCalledWithExpectedStartDate(DateTime startDate)
    {
        _overlapCheckService.Verify(x =>
            x.CheckForOverlaps(It.IsAny<string>(), It.Is<DateRange>(dr => dr.From == startDate), It.IsAny<long?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void VerifyReservationValidationServiceIsCalledWithExpectedStartDate(DateTime startDate)
    {
        _reservationValidationService.Verify(x => x.Validate(It.Is<ReservationValidationRequest>(x => x.StartDate == startDate), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void WithPriceHistoryWithStartDate(decimal cost)
    {
        Apprenticeship.PriceHistory = new List<PriceHistory>
        {
            new()
            {
                FromDate = Apprenticeship.StartDate.Value.AddMonths(-1),
                ToDate = null,
                Cost = cost
            }
        };
    }

    private EditApprenticeshipValidationServiceTestsFixture CreateMockApprenticeshipContext()
    {
        var apprenticeships = new List<Apprenticeship> { Apprenticeship };

        _dbContext.Setup(x => x.Apprenticeships).ReturnsDbSet(apprenticeships);

        return this;
    }

    private EditApprenticeshipValidationServiceTestsFixture CreateApprenticeship(long id = 100,
        long commitmentId = 200,
        string firstName = "FirstName",
        string lastName = "lastName",
        string email = null,
        int dobYear = 1995,
        int dobMonth = 1,
        int dobDay = 1,
        string employerRef = "employerRef",
        string uln = "XYZ123",
        string courseCode = "12",
        ProgrammeType programmeType = ProgrammeType.Standard,
        int? transferSenderId = null,
        decimal cost = 200,
        string reservationId = "134463EF-0088-4828-8775-EBD1223486AF",
        PaymentStatus paymentStatus = PaymentStatus.Active,
        bool hasHadDataLockSuccess = false,
        DateTime employerProviderApprovedOn = default,
        DeliveryModel deliveryModel = DeliveryModel.Regular,
        FlexibleEmployment flexibleEmployment = null,
        bool isOnFlexiPaymentsPilot = false
    )
    {
        Apprenticeship = new Apprenticeship
        {
            Id = id,
            CommitmentId = commitmentId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = new DateTime(dobYear, dobMonth, dobDay),
            Cost = cost,
            EmployerRef = employerRef,
            DeliveryModel = deliveryModel,
            ProgrammeType = programmeType,
            CourseCode = courseCode,
            ReservationId = Guid.Parse(reservationId),
            Cohort = new Cohort
            {
                Id = commitmentId,
                TransferSenderId = transferSenderId,
                EmployerAndProviderApprovedOn = employerProviderApprovedOn
            },
            Uln = uln,
            PaymentStatus = paymentStatus,
            HasHadDataLockSuccess = hasHadDataLockSuccess,
            FlexibleEmployment = flexibleEmployment,
            IsOnFlexiPaymentPilot = isOnFlexiPaymentsPilot
        };

        return this;
    }

    public EditApprenticeshipValidationRequest CreateValidationRequest(
        long id = 100,
        long employerAccountId = 250,
        string firstName = "",
        string lastName = "",
        string email = null,
        int? dobYear = null,
        int? dobMonth = null,
        int? dobDay = null,
        int? startMonth = null,
        int? startYear = null,
        int? endMonth = null,
        int? endYear = null,
        string employerRef = "",
        string uln = "",
        string courseCode = "",
        decimal? cost = null,
        string providerRef = "",
        DeliveryModel deliveryModel = DeliveryModel.Regular,
        int? employmentEndMonth = null,
        int? employmentEndYear = null,
        int? employmentPrice = null,
        DateTime? actualStartDate = null
    )
    {
        var request = new EditApprenticeshipValidationRequest
        {
            ApprenticeshipId = id,
            EmployerAccountId = employerAccountId,
            FirstName = string.IsNullOrEmpty(firstName) ? Apprenticeship.FirstName : firstName,
            LastName = string.IsNullOrEmpty(lastName) ? Apprenticeship.LastName : lastName,
            Email = email,
            EndDate = null,
            DateOfBirth = null,
            StartDate = null,
            Cost = cost.HasValue ? cost : Apprenticeship.Cost,
            EmployerReference = string.IsNullOrEmpty(employerRef) ? Apprenticeship.EmployerRef : employerRef,
            ProviderReference = string.IsNullOrEmpty(providerRef) ? Apprenticeship.ProviderRef : providerRef,
            CourseCode = string.IsNullOrEmpty(courseCode) ? Apprenticeship.CourseCode : courseCode,
            ULN = string.IsNullOrEmpty(uln) ? Apprenticeship.Uln : uln,
            DeliveryModel = deliveryModel,
            EmploymentEndDate = null,
            EmploymentPrice = employmentPrice ?? Apprenticeship.FlexibleEmployment?.EmploymentPrice
        };

        if (dobYear.HasValue && dobMonth.HasValue && dobDay.HasValue)
        {
            request.DateOfBirth = new DateTime(dobYear.Value, dobMonth.Value, dobDay.Value);
        }
        else
        {
            request.DateOfBirth = Apprenticeship.DateOfBirth;
        }

        if (startYear.HasValue && startMonth.HasValue)
        {
            request.StartDate = new DateTime(startYear.Value, startMonth.Value, 1);
        }
        else
        {
            request.StartDate = Apprenticeship.StartDate;
        }

        request.ActualStartDate = actualStartDate ?? Apprenticeship.ActualStartDate;

        if (endYear.HasValue && endMonth.HasValue)
        {
            request.EndDate = new DateTime(endYear.Value, endMonth.Value, 1);
        }
        else
        {
            request.EndDate = Apprenticeship.EndDate;
        }

        if (employmentEndYear.HasValue && employmentEndMonth.HasValue)
        {
            request.EmploymentEndDate = new DateTime(employmentEndYear.Value, employmentEndMonth.Value, 1);
        }
        else
        {
            request.EmploymentEndDate = Apprenticeship.FlexibleEmployment?.EmploymentEndDate;
        }

        return request;
    }

    public Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request)
    {
        return _sut.Validate(request, CancellationToken.None);
    }
}