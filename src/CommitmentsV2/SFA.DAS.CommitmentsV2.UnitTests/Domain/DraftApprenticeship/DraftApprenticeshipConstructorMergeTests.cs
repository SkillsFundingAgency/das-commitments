using System;
using System.Collections.Generic;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.DraftApprenticeship
{
    [TestFixture]
    public class DraftApprenticeshipConstructorMergeTests
    {
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
        public void ThenUlnIsMappedCorrectly()
        {
            CheckPropertyAfterMerge(update => update.Uln, result => result.Uln);
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
            var fixture = new Fixture();
            var original = fixture.Create<DraftApprenticeshipDetails>();
            var draftApprenticeship = new CommitmentsV2.Models.DraftApprenticeship(original, Party.Employer);
            var update = fixture.Create<DraftApprenticeshipDetails>();

            draftApprenticeship.Merge(update, modifyingParty);

            var expectedValue = expected(update);
            var actualValue = actual(draftApprenticeship);

            IEqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;

            Assert.IsTrue(comparer.Equals(expectedValue, actualValue),
                $"Expected value: {expectedValue} Actual value: {actualValue}");
        }
    }
}
