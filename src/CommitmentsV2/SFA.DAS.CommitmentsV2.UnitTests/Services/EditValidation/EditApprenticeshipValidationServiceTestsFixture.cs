using MediatR;
using Moq;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EditApprenticeshipValidationServiceTestsFixture
    {
        private EditApprenitceshipValidationService _sut;
        private Mock<IProviderCommitmentsDbContext> _dbContext;
        private Mock<IMediator> _mediator;
        private Mock<IOverlapCheckService> _overlapCheckService;
        private Mock<IReservationValidationService> _reservationValidationService;
        private Mock<IAcademicYearDateProvider> _academicYearDateProvider;
        private Mock<ICurrentDateTime> _currentDateTime;

        public DateTime? StartDate { get
            {
                return _apprenticeship.StartDate;
            }
        }

        internal DateTime GetEndOfCurrentTeachingYear()
        {
            return _academicYearDateProvider.Object.CurrentAcademicYearEndDate;
        }

        private Apprenticeship _apprenticeship;

        public EditApprenticeshipValidationServiceTestsFixture()
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

            _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long>(), CancellationToken.None))
                .Returns(Task.FromResult(new OverlapCheckResult(false, false))) ;

            _currentDateTime.Setup(x => x.UtcNow).Returns(() => new DateTime(2021, 3, 19));

            _reservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ReservationValidationResult(new ReservationValidationError[0])));
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
                    new ReservationValidationError("CourseCode","Reason")
                })));
            return this;
        }

        internal EditApprenticeshipValidationServiceTestsFixture CourseIsEffectiveFromDate(DateTime effectiveFrom, int activeForInYears = 5, ProgrammeType programmeType = ProgrammeType.Standard)
        {
            _mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(new GetTrainingProgrammeQueryResult()
                {
                    TrainingProgramme = new Types.TrainingProgramme
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
            return _apprenticeship.StartDate.Value;
        }
  
        public EditApprenticeshipValidationServiceTestsFixture SetupOverlapService(bool startDateOverlaps, bool endDateOverlaps)
        {
            _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long>(), CancellationToken.None))
              .Returns(Task.FromResult(new OverlapCheckResult(startDateOverlaps, endDateOverlaps)));
            return this;
        }

        private void WithStartDateInFuture()
        {
            _apprenticeship.StartDate = _currentDateTime.Object.UtcNow.AddMonths(1);
            _apprenticeship.EndDate = _currentDateTime.Object.UtcNow.AddYears(1);
        }

        private EditApprenticeshipValidationServiceTestsFixture WithInFundingPeriod()
        {
            _academicYearDateProvider.Setup(t => t.CurrentAcademicYearStartDate).Returns(_apprenticeship.StartDate.Value.AddMonths(-1));

            _academicYearDateProvider.Setup(t => t.CurrentAcademicYearEndDate).Returns(_apprenticeship.StartDate.Value.AddYears(1));

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
            Types.ProgrammeType programmeType = Types.ProgrammeType.Standard,
            int? transferSenderId = null,
            decimal cost = 200,
            string reservationId = "134463EF-0088-4828-8775-EBD1223486AF",
            Types.PaymentStatus paymentStatus = Types.PaymentStatus.Active,
            bool hasHadDataLockSuccess = false
            )
        {
            CreateApprenticeship(id, commitmentId, firstName, lastName, email, dobYear, dobMonth, dobDay, employerRef, uln, courseCode, programmeType, transferSenderId, cost, reservationId, paymentStatus, hasHadDataLockSuccess);

            WithStartDateInFuture();

            WithInFundingPeriod();

            WithPriceHistoryWithStartDate(cost);

            CreateMockApprenticeshipContext();
            return this;
        }

        private void WithPriceHistoryWithStartDate(decimal cost)
        {
            _apprenticeship.PriceHistory = new List<PriceHistory>
            {
                new PriceHistory
                {
                    FromDate = _apprenticeship.StartDate.Value.AddMonths(-1),
                    ToDate = null,
                    Cost = cost
                }
            };
        }

        private EditApprenticeshipValidationServiceTestsFixture CreateMockApprenticeshipContext()
        {
            List<Apprenticeship> apprenticeships = new List<Apprenticeship>()
            {
              _apprenticeship
            };

            _dbContext.Setup(x => x.Apprenticeships).ReturnsDbSet(apprenticeships);

            return this;
        }

        public EditApprenticeshipValidationServiceTestsFixture CreateApprenticeship(long id = 100,
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
            Types.ProgrammeType programmeType = Types.ProgrammeType.Standard,
            int? transferSenderId = null,
            decimal cost = 200,
            string reservationId = "134463EF-0088-4828-8775-EBD1223486AF",
            Types.PaymentStatus paymentStatus = Types.PaymentStatus.Active,
            bool hasHadDataLockSuccess = false)
        {
           _apprenticeship = new Apprenticeship
            {
                Id = id,
                CommitmentId = commitmentId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                DateOfBirth = new DateTime(dobYear, dobMonth, dobDay),
                Cost = cost,
                EmployerRef = employerRef,
                ProgrammeType = programmeType,
                CourseCode = courseCode,
                ReservationId = Guid.Parse(reservationId),
                Cohort = new Cohort
                {
                    Id = commitmentId,
                    TransferSenderId = transferSenderId
                },
                Uln = uln,
                PaymentStatus = paymentStatus,
                HasHadDataLockSuccess = hasHadDataLockSuccess
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
           decimal? cost = null
           )
        {
            var request = new EditApprenticeshipValidationRequest
            {
                ApprenticeshipId = id,
                EmployerAccountId = employerAccountId,
                FirstName = string.IsNullOrEmpty(firstName) ? _apprenticeship.FirstName : firstName,
                LastName = string.IsNullOrEmpty(lastName) ? _apprenticeship.LastName : lastName,
                Email = email,
                EndDate = null,
                DateOfBirth = null,
                StartDate = null,
                Cost = cost.HasValue ? cost : _apprenticeship.Cost,
                EmployerReference = string.IsNullOrEmpty(employerRef) ? _apprenticeship.EmployerRef : employerRef,
                CourseCode = string.IsNullOrEmpty(courseCode) ? _apprenticeship.CourseCode : courseCode,
                ULN = string.IsNullOrEmpty(uln) ? _apprenticeship.Uln : uln,
            };

            if (dobYear.HasValue && dobMonth.HasValue && dobDay.HasValue)
            {
                request.DateOfBirth = new DateTime(dobYear.Value, dobMonth.Value, dobDay.Value);
            }
            else
            {
                request.DateOfBirth = _apprenticeship.DateOfBirth;
            }

            if (startYear.HasValue && startMonth.HasValue)
            {
                request.StartDate = new DateTime(startYear.Value, startMonth.Value, 1);
            }
            else
            {
                request.StartDate = _apprenticeship.StartDate;
            }

            if (endYear.HasValue && endMonth.HasValue)
            {
                request.EndDate = new DateTime(endYear.Value, endMonth.Value, 1);
            }
            else
            {
                request.EndDate = _apprenticeship.EndDate;
            }

            return request;
        }

         public Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request)
        {
            return _sut.Validate(request, CancellationToken.None);
        }
    }
}
