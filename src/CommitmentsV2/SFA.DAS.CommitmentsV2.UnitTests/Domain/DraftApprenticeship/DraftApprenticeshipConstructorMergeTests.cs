using System;
using System.Collections.Generic;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.DraftApprenticeship
{
    [TestFixture]
    public class DraftApprenticeshipConstructorMergeTests
    {
        private Fixture _fixture;
        private DraftApprenticeshipDetails _original;
        private CommitmentsV2.Models.DraftApprenticeship _draftApprenticeship;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _original = _fixture.Create<DraftApprenticeshipDetails>();
            _draftApprenticeship = new CommitmentsV2.Models.DraftApprenticeship(_original, Party.Provider);
        }

        [Test]
        public void ThenFirstNameIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.FirstName, result => result.FirstName);
        }

        [Test]
        public void ThenLastNameIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.LastName, result => result.LastName);
        }

        [Test]
        public void ThenUlnIsMappedCorrectlyForProvider()
        {
            CheckPropertyAfterMerge(update => update.Uln, result => result.Uln, Party.Provider);
        }

        [Test]
        public void ThenUlnIsNeMappedForEmployer()
        {
            CheckPropertyAfterMerge(update => update.Uln, result => result.Uln, Party.Employer);
        }

        [Test]
        public void ThenExceptionIsThrownIfUlnIsChangedByEmployer()
        {
            DraftApprenticeshipDetails update = _fixture.Create<DraftApprenticeshipDetails>();
            Assert.Throws<DomainException>(() => _draftApprenticeship.Merge(update, Party.Employer));
        }

        [Test]
        public void ThenCostIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.Cost, result => result.Cost);
        }

        [Test]
        public void ThenStartDateIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.StartDate, result => result.StartDate);
        }

        [Test]
        public void ThenEndDateIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.EndDate, result => result.EndDate);
        }

        [Test]
        public void ThenDateOfBirthIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.DateOfBirth, result => result.DateOfBirth);
        }

        [Test]
        public void ThenProgrammeTypeIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.TrainingProgramme.ProgrammeType, result => result.ProgrammeType);
        }

        [Test]
        public void ThenCourseCodeIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.TrainingProgramme.CourseCode, result => result.CourseCode);
        }

        [Test]
        public void ThenCourseNameIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.TrainingProgramme.Name, result => result.CourseName);
        }

        [Test]
        public void ThenProviderReferenceIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.Reference, result => result.ProviderRef, modifyingParty: Party.Provider);
        }

        [Test]
        public void ThenEmployerReferenceIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.Reference, result => result.EmployerRef, modifyingParty: Party.Employer);
        }

        private void CheckPropertyAfterMerge<TValue>(Func<DraftApprenticeshipDetails, TValue> expected,
            Func<CommitmentsV2.Models.DraftApprenticeship, TValue> actual, Party modifyingParty = Party.Provider)
        {
            var update = CreateApprenticeshipUpdateDetails(modifyingParty);

            _draftApprenticeship.Merge(update, modifyingParty);

            var expectedValue = expected(update);
            var actualValue = actual(_draftApprenticeship);

            IEqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;

            Assert.IsTrue(comparer.Equals(expectedValue, actualValue),
                $"Expected value: {expectedValue} Actual value: {actualValue}");
        }

        private DraftApprenticeshipDetails CreateApprenticeshipUpdateDetails(Party modifyingParty)
        {
            if (modifyingParty == Party.Provider)
            {
                return _fixture.Create<DraftApprenticeshipDetails>();
            }
            return _fixture.Build<DraftApprenticeshipDetails>().With(x => x.Uln, _original.Uln).Create();
        }
    }
}