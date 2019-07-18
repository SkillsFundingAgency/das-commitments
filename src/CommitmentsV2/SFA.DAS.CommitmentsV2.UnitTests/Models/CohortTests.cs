﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork;

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
            var originalDraft = fixtures.Create();
            var modifiedDraft = fixtures.UpdatePropertiesWithNewValues(originalDraft);
            var modifiedDraftDetails = fixtures.ToApprenticeshipDetails(modifiedDraft, Party.Provider);

            var c = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly};
            c.Apprenticeships.Add(originalDraft);

            // Act
            c.UpdateDraftApprenticeship(modifiedDraftDetails, Party.Provider, fixtures.UserInfo);

            // Assert
            var savedDraft = c.DraftApprenticeships.Single(a => a.Id == modifiedDraft.Id);
            fixtures.AssertSameProperties(modifiedDraft, savedDraft);
        }

        [Test]
        public void UpdateDraftApprenticeship_WithInvalidInput_ShouldThrowException()
        {
            var fixtures = new CohortTestFixtures();

            // arrange
            var originalDraft = fixtures.Create();
            var modifiedDraft = fixtures.UpdatePropertiesWithNewValues(originalDraft);
            var modifiedDraftDetails = fixtures.ToApprenticeshipDetails(modifiedDraft, Party.Provider);
            modifiedDraftDetails.StartDate = modifiedDraftDetails.EndDate.Value.AddMonths(1);

            var c = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly};
            c.Apprenticeships.Add(originalDraft);

            // Act
            Assert.Throws<DomainException>(() => c.UpdateDraftApprenticeship(modifiedDraftDetails, Party.Provider, fixtures.UserInfo));
        }
    }

    class CohortTestFixtures
    {
        public CohortTestFixtures()
        {
            var autoFixture = new Fixture();

            // We need this to allow the UoW to initialise it's internal static events collection.
            var uow = new UnitOfWorkContext();
            UserInfo = autoFixture.Create<UserInfo>();
        }

        public UserInfo UserInfo { get;  }

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
            }, Party.Provider);
        }

        public DraftApprenticeship UpdatePropertiesWithNewValues(DraftApprenticeship draftApprenticeship)
        {
            return new DraftApprenticeship(new DraftApprenticeshipDetails
            {
                Id = draftApprenticeship.Id,
                FirstName = SafeUpdate(draftApprenticeship.FirstName),
                LastName = SafeUpdate(draftApprenticeship.LastName),
                StartDate = SafeUpdate(draftApprenticeship.StartDate),
                EndDate = SafeUpdate(draftApprenticeship.EndDate),
                DateOfBirth = SafeUpdate(draftApprenticeship.DateOfBirth)
            }, Party.Provider);
        }

        public DraftApprenticeshipDetails ToApprenticeshipDetails(DraftApprenticeship draftApprenticeship,
            Party modificationParty)
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

        private void AssertSameProperty<P>(DraftApprenticeship expected, DraftApprenticeship actual,
            Expression<Func<DraftApprenticeship, P>> action)
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