using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort
{
    [TestFixture]
    [Parallelizable]
    public class UpdateDraftApprenticeshipValidationTests
    {
        private UpdateDraftApprenticeshipValidationTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new UpdateDraftApprenticeshipValidationTestsFixture().WithProviderCohort();
        }

        [TestCase(1, "", "", true)]
        [TestCase(1, "AAA111", "AAA111", true)]
        [TestCase(1, "AAA111", "BBB222", true)]
        public void Uln_CheckNoDuplicates_Validation(long existingId, string existingUln, string uln, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithApprenticeship(existingId, existingUln).WithId(existingId).WithUln(uln),
                nameof(_fixture.DraftApprenticeshipDetails.Uln),
                passes);
        }
    }

    public class UpdateDraftApprenticeshipValidationTestsFixture
    {
        public UnitOfWorkContext UnitOfWorkContext;
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public CommitmentsV2.Models.Cohort Cohort;
        public ICurrentDateTime CurrentDateTime;
        public IAcademicYearDateProvider AcademicYearDateProvider;
        public UserInfo UserInfo { get; }

        public UpdateDraftApprenticeshipValidationTestsFixture()
        {
            var autoFixture = new Fixture();
            UnitOfWorkContext = new UnitOfWorkContext();
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue)
            };
            SetupMinimumNameProperties();
            Cohort = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly, ProviderId = 1};
            CurrentDateTime = new CurrentDateTime(new DateTime(2019, 04, 01, 0, 0, 0, DateTimeKind.Utc));
            AcademicYearDateProvider = new AcademicYearDateProvider(CurrentDateTime);
            UserInfo = autoFixture.Create<UserInfo>();
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithProviderCohort()
        {
            Cohort = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly, ProviderId = 1 };
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithEmployerCohort()
        {
            Cohort = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.EmployerOnly, ProviderId = 1 };
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture SetupMinimumNameProperties()
        {
            DraftApprenticeshipDetails.FirstName = "TestFirstName";
            DraftApprenticeshipDetails.LastName = "TestLastName";
            return this;
        }

        public void AssertValidationForProperty(Action setup, string propertyName, bool expected)
        {
            setup();

            try
            {
                Cohort.UpdateDraftApprenticeship(DraftApprenticeshipDetails, Party.Provider, UserInfo);
                Assert.AreEqual(expected, true);
            }
            catch (DomainException ex)
            {
                Assert.AreEqual(expected, false);
                Assert.Contains(propertyName, ex.DomainErrors.Select(x => x.PropertyName).ToList());
            }
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithCurrentDate(DateTime currentDate)
        {
            var utcCurrentDate = DateTime.SpecifyKind(currentDate, DateTimeKind.Utc);
            CurrentDateTime = new CurrentDateTime(utcCurrentDate);
            AcademicYearDateProvider = new AcademicYearDateProvider(CurrentDateTime);
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithStartDate(DateTime? startDate)
        {
            DraftApprenticeshipDetails.StartDate = startDate;
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithTrainingProgrammeEffectiveBetween(DateTime startDate, DateTime endDate)
        {
            DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme("TEST",
                "TEST",
                ProgrammeType.Framework,
                DateTime.SpecifyKind(startDate,DateTimeKind.Utc),
                DateTime.SpecifyKind(endDate,DateTimeKind.Utc));
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithApprenticeship(long id, string uln)
        {
            var draftApprenticeshipDetails = new DraftApprenticeshipDetails().Set(d => d.Uln, uln);
            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, Party.Provider).Set(d => d.Id, id);
            
            Cohort.Apprenticeships.Add(draftApprenticeship);
            
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithId(long id)
        {
            DraftApprenticeshipDetails.Id = id;
            
            return this;
        }

        public UpdateDraftApprenticeshipValidationTestsFixture WithUln(string uln)
        {
            DraftApprenticeshipDetails.Uln = uln;
            
            return this;
        }
    }
}