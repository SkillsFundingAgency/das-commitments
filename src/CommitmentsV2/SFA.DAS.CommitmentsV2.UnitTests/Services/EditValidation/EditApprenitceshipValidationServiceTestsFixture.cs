using MediatR;
using Moq;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EditApprenitceshipValidationServiceTestsFixture
    {
        private EditApprenitceshipValidationService _sut;
        private Mock<IProviderCommitmentsDbContext> _dbContext;
        private Mock<IMediator> _mediator;
        private Mock<IOverlapCheckService> _overlapCheckService;
        private Mock<IReservationValidationService> _reservationValidationService;
        private Mock<IAcademicYearDateProvider> _academicYearDateProvider;
        private Mock<ICurrentDateTime> _currentDateTime;

        public EditApprenitceshipValidationServiceTestsFixture()
        {
            _dbContext = new Mock<IProviderCommitmentsDbContext>();
            _mediator = new Mock<IMediator>();
            _overlapCheckService = new Mock<IOverlapCheckService>();
            _reservationValidationService = new Mock<IReservationValidationService>();
            _academicYearDateProvider = new Mock<IAcademicYearDateProvider>();
            _currentDateTime = new Mock<ICurrentDateTime>();

            _sut = new EditApprenitceshipValidationService(_dbContext.Object, _mediator.Object,
                _overlapCheckService.Object,
                _reservationValidationService.Object,
                _academicYearDateProvider.Object,
                _currentDateTime.Object);

            _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long>(), CancellationToken.None))
                .Returns(Task.FromResult(new OverlapCheckResult(false, false))) ;

            //_currentDateTime.Setup(x => x.UtcNow).Returns(() => new DateTime(2021, 3, 19));

            _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ReservationValidationResult(new ReservationValidationError[0])));
        }

        public EditApprenitceshipValidationServiceTestsFixture SetupReservationValidationService()
        {
            _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ReservationValidationResult(new ReservationValidationError[1] { 
                    new ReservationValidationError("CourseCode","Reason")
                })));
            return this;
        }

        public EditApprenitceshipValidationServiceTestsFixture SetupMockAcademicYearDateProvider(DateTime currentAcademicYearStartDate)
        {
            _academicYearDateProvider.Setup(x => x.CurrentAcademicYearStartDate).Returns(currentAcademicYearStartDate);
            _academicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(currentAcademicYearStartDate.AddYears(1).AddDays(-1));
            _academicYearDateProvider.Setup(x => x.LastAcademicYearFundingPeriod).Returns(new DateTime(currentAcademicYearStartDate.Year, 10, 19, 18, 0, 0, DateTimeKind.Utc));

            return this;
        }

        public EditApprenitceshipValidationServiceTestsFixture SetUpMediatorForTrainingCourse(DateTime effectiveFrom, int activeForInYears = 5, ProgrammeType programmeType = ProgrammeType.Standard)
        {
            _mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(new GetTrainingProgrammeQueryResult() {
                TrainingProgramme = new Types.TrainingProgramme
                {
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = effectiveFrom.AddYears(activeForInYears),
                    ProgrammeType = programmeType
                }
            }));

            return this;
        }

        public EditApprenitceshipValidationServiceTestsFixture SetupCurrentDateTime(DateTime currentDateTime)
        {
            _currentDateTime.Setup(x => x.UtcNow).Returns(currentDateTime);
            return this;
        }

        public EditApprenitceshipValidationServiceTestsFixture SetupOverlapService(bool startDateOverlaps, bool endDateOverlaps)
        {
            _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long>(), CancellationToken.None))
              .Returns(Task.FromResult(new OverlapCheckResult(startDateOverlaps, endDateOverlaps)));
            return this;
        }

        public EditApprenitceshipValidationServiceTestsFixture SetupMockContextApprenitceship(
            long id = 100,
            long commitmentId = 200,
            string firstName = "FirstName",
            string lastName = "lastName",
            int dobYear = 1995,
            int dobMonth = 1,
            int dobDay = 1,
            int startMonth = 1,
            int startYear = 2020,
            int endMonth = 1,
            int endYear = 2021,
            string employerRef = "employerRef",
            string uln = "XYZ123",
            string courseCode = "12",
            Types.ProgrammeType programmeType = Types.ProgrammeType.Standard,
            int? transferSenderId = null,
            decimal cost = 200,
            string reservationId = "134463EF-0088-4828-8775-EBD1223486AF",
            Types.PaymentStatus paymentStatus = Types.PaymentStatus.Active,
            bool hasHadDataLockSuccess = false
            )
        {
            List<CommitmentsV2.Models.Apprenticeship> apprenticeship = new List<CommitmentsV2.Models.Apprenticeship>()
            {
                new CommitmentsV2.Models.Apprenticeship
                {
                    Id = id,
                    CommitmentId = commitmentId,
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = new DateTime(dobYear,dobMonth,dobDay),
                    StartDate = new DateTime(startYear,startMonth,1),
                    EndDate = new DateTime(endYear,endMonth,1),
                    Cost = cost,
                    EmployerRef = employerRef,
                    ProgrammeType = programmeType,
                    CourseCode = courseCode,
                    ReservationId = Guid.Parse(reservationId),
                    Cohort = new CommitmentsV2.Models.Cohort
                    {
                        Id = commitmentId,
                        TransferSenderId = transferSenderId
                    },
                    Uln = uln,
                    PaymentStatus = paymentStatus,
                    HasHadDataLockSuccess = hasHadDataLockSuccess
                }
            };

            _dbContext.Setup(x => x.Apprenticeships).ReturnsDbSet(apprenticeship);
            return this;
        }

        public EditApprenticeshipValidationRequest CreateValidationRequest(
           long id = 100,
           long employerAccountId = 250,
           string firstName = "FirstName",
           string lastName = "lastName",
           int? dobYear = 1995,
           int dobMonth = 1,
           int dobDay = 1,
           int startMonth = 1,
           int? startYear = 2020,
           int endMonth = 1,
           int? endYear = 2021,
           string employerRef = "employerRef",
           string uln = "XYZ123",
           string courseCode = "12",
           decimal? cost = 200
           )
        {
            var request = new EditApprenticeshipValidationRequest
            {
                ApprenticeshipId = id,
                EmployerAccountId = employerAccountId,
                FirstName = firstName,
                LastName = lastName,
                EndDate = null,
                DateOfBirth = null,
                StartDate = null,
                Cost = cost,
                EmployerReference = employerRef,
                TrainingCode = courseCode, // TODO: change the training code to couseCode
                ULN = uln
            };

            if (dobYear.HasValue)
            {
                request.DateOfBirth = new DateTime(dobYear.Value, dobMonth, dobDay);
            }

            if (startYear.HasValue)
            {
                request.StartDate = new DateTime(startYear.Value, startMonth, 1);
            }

            if (endYear.HasValue)
            {
                request.EndDate = new DateTime(endYear.Value, endMonth, 1);
            }

            return request;
        }

         public Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request)
        {
            return _sut.Validate(request, CancellationToken.None);
        }
    }
}
