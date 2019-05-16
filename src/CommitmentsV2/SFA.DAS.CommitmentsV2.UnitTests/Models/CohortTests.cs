using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models
{
    [TestFixture]
    public class CohortTests
    {
        [Test]
        public void UpdateDraftApprenticeship_WithValidInput_ShouldUpdateExistingDraftApprenticeship()
        {
            var fixtures = new CohortTestFixtures();

            // arrange
            var existingDraft = fixtures.Create();
            var newDraft = fixtures.Update(existingDraft);
            var newDraftDetails = fixtures.ToApprenticeshipDetails(newDraft);

            var c = new Cohort();    
            c.Apprenticeships.Add(existingDraft);

            // Act
            c.UpdateDraftApprenticeship(newDraftDetails);

            // Assert
            var updatedDraft = c.DraftApprenticeships.Single(a => a.Id == newDraft.Id);
            fixtures.AssertSameProperties(newDraft, updatedDraft);
        }

        [Test]
        public void UpdateDraftApprenticeship_WithInvalidInput_ShouldThrowException()
        {
            var fixtures = new CohortTestFixtures();

            // arrange
            var existingDraft = fixtures.Create();
            var newDraft = fixtures.Update(existingDraft);
            var newDraftDetails = fixtures.ToApprenticeshipDetails(newDraft);
            newDraftDetails.StartDate = newDraftDetails.EndDate.Value.AddMonths(1);

            var c = new Cohort();
            c.Apprenticeships.Add(existingDraft);

            // Act
            Assert.Throws<DomainException>(() => c.UpdateDraftApprenticeship(newDraftDetails));
        }
    }

    class CohortTestFixtures
    {
        public CohortTestFixtures()
        {
            
        }

        public DraftApprenticeship Create()
        {
            return new DraftApprenticeship(new DraftApprenticeshipDetails
                {
                    Id = new Random().Next(100, 200),
                    FirstName = "ABC",
                    LastName = "EFG",
                    StartDate = DateTime.Today.AddMonths(-6),
                    EndDate = DateTime.Today.AddMonths(6),
                    DateOfBirth = DateTime.Today.AddYears(-18)
                }, Originator.Provider);
        }

        public DraftApprenticeship Update(DraftApprenticeship draftApprenticeship)
        {
            return new DraftApprenticeship(new DraftApprenticeshipDetails
            {
                Id = draftApprenticeship.Id,
                FirstName = SafeUpdate(draftApprenticeship.FirstName),
                LastName = SafeUpdate(draftApprenticeship.LastName),
                StartDate = SafeUpdate(draftApprenticeship.StartDate),
                EndDate = SafeUpdate(draftApprenticeship.EndDate),
                DateOfBirth = SafeUpdate(draftApprenticeship.DateOfBirth)
            }, Originator.Provider);
        }

        public DraftApprenticeshipDetails ToApprenticeshipDetails(DraftApprenticeship draftApprenticeship)
        {
            return new DraftApprenticeshipDetails
            {
                Id = draftApprenticeship.Id,
                FirstName = draftApprenticeship.FirstName,
                LastName = draftApprenticeship.LastName,
                Uln = draftApprenticeship.Uln,
                TrainingProgramme = new TrainingProgramme(draftApprenticeship.CourseCode, "", ProgrammeType.Framework,
                    null, null),
                Cost = (int?) draftApprenticeship.Cost,
                StartDate = draftApprenticeship.StartDate,
                EndDate = draftApprenticeship.EndDate,
                DateOfBirth = draftApprenticeship.DateOfBirth,
                Reference = draftApprenticeship.ProviderRef,
                ReservationId = draftApprenticeship.ReservationId
            };
        }


        public void AssertSameProperties(DraftApprenticeship expected, DraftApprenticeship actual)
        {
            AssertSameProperty(expected, actual, da => da.StartDate);
            AssertSameProperty(expected, actual, da => da.DateOfBirth);
            AssertSameProperty(expected, actual, da => da.EndDate);
            AssertSameProperty(expected, actual, da => da.Cost);
            AssertSameProperty(expected, actual, da => da.CourseCode);
            AssertSameProperty(expected, actual, da => da.Id);
            AssertSameProperty(expected, actual, da => da.ReservationId);
            AssertSameProperty(expected, actual, da => da.Uln);
            AssertSameProperty(expected, actual, da => da.FirstName);
            AssertSameProperty(expected, actual, da => da.LastName);
        }

        private void AssertSameProperty<P>(DraftApprenticeship expected, DraftApprenticeship actual, Expression<Func<DraftApprenticeship, P>> action)
        {
            var expression = (MemberExpression) action.Body;
            string name = expression.Member.Name;

            var propertyInfo = expression.Member as PropertyInfo;

            var expectedValue = propertyInfo.GetValue(expected);
            var actualValue = propertyInfo.GetValue(actual);

            Assert.AreEqual(expectedValue, actualValue, $"{name} is not equal");
        }

        private string SafeUpdate(string src)
        {
            return (src ?? "") + "X";
        }

        private DateTime SafeUpdate(DateTime? src)
        {
            return (src ?? DateTime.Today).AddDays(1);
        }
    }
}
