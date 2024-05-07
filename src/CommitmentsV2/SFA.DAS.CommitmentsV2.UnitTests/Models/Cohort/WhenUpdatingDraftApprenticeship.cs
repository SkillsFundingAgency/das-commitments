using System.Linq.Expressions;
using System.Reflection;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort;

[TestFixture]
public class WhenUpdatingDraftApprenticeship
{
    [Test]
    public void UpdateDraftApprenticeship_WithValidInput_ShouldUpdateExistingDraftApprenticeship()
    {
        var fixtures = new CohortTestFixtures();

        // arrange
        var originalDraft = CohortTestFixtures.Create();
        var modifiedDraft = CohortTestFixtures.UpdatePropertiesWithNewValues(originalDraft);
        var modifiedDraftDetails = CohortTestFixtures.ToApprenticeshipDetails(modifiedDraft, Party.Provider);

        var cohort = new CommitmentsV2.Models.Cohort {WithParty = Party.Provider, ProviderId = 1};
        cohort.Apprenticeships.Add(originalDraft);

        // Act
        cohort.UpdateDraftApprenticeship(modifiedDraftDetails, Party.Provider, fixtures.UserInfo);

        // Assert
        var savedDraft = cohort.DraftApprenticeships.Single(a => a.Id == modifiedDraft.Id);
        CohortTestFixtures.AssertSameProperties(modifiedDraft, savedDraft);
    }

    [Test]
    public void UpdateDraftApprenticeship_WithInvalidInput_ShouldThrowException()
    {
        var fixtures = new CohortTestFixtures();

        // arrange
        var originalDraft = CohortTestFixtures.Create();
        var modifiedDraft = CohortTestFixtures.UpdatePropertiesWithNewValues(originalDraft);
        var modifiedDraftDetails = CohortTestFixtures.ToApprenticeshipDetails(modifiedDraft, Party.Provider);
        modifiedDraftDetails.StartDate = modifiedDraftDetails.EndDate.Value.AddMonths(1);

        var cohort = new CommitmentsV2.Models.Cohort {WithParty = Party.Provider, ProviderId = 1};
        cohort.Apprenticeships.Add(originalDraft);

        // Act
        Assert.Throws<DomainException>(() => cohort.UpdateDraftApprenticeship(modifiedDraftDetails, Party.Provider, fixtures.UserInfo));
    }
}

internal class CohortTestFixtures
{
    public CohortTestFixtures()
    {
        var autoFixture = new Fixture();

        // We need this to allow the UoW to initialise it's internal static events collection.
        var uow = new UnitOfWorkContext();
        UserInfo = autoFixture.Create<UserInfo>();
    }

    public UserInfo UserInfo { get;  }

    public static DraftApprenticeship Create()
    {
        return new DraftApprenticeship(new DraftApprenticeshipDetails
        {
            Id = new Random().Next(100, 200),
            FirstName = "ABC",
            LastName = "EFG",
            StartDate = DateTime.Today.AddMonths(-6),
            EndDate = DateTime.Today.AddMonths(6),
            DateOfBirth = DateTime.Today.AddYears(-18),
            DeliveryModel = DeliveryModel.Regular,
            IsOnFlexiPaymentPilot = false
        }, Party.Provider);
    }

    public static DraftApprenticeship UpdatePropertiesWithNewValues(DraftApprenticeship draftApprenticeship)
    {
        return new DraftApprenticeship(new DraftApprenticeshipDetails
        {
            Id = draftApprenticeship.Id,
            FirstName = SafeUpdate(draftApprenticeship.FirstName),
            LastName = SafeUpdate(draftApprenticeship.LastName),
            StartDate = SafeUpdate(draftApprenticeship.StartDate),
            EndDate = SafeUpdate(draftApprenticeship.EndDate),
            DateOfBirth = SafeUpdate(draftApprenticeship.DateOfBirth),
            DeliveryModel = draftApprenticeship.DeliveryModel,
            IsOnFlexiPaymentPilot = draftApprenticeship.IsOnFlexiPaymentPilot
        }, Party.Provider);
    }

    public static DraftApprenticeshipDetails ToApprenticeshipDetails(DraftApprenticeship draftApprenticeship, Party modificationParty)
    {
        return new DraftApprenticeshipDetails
        {
            Id = draftApprenticeship.Id,
            FirstName = draftApprenticeship.FirstName,
            LastName = draftApprenticeship.LastName,
            Uln = draftApprenticeship.Uln,
            TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme(draftApprenticeship.CourseCode, "", ProgrammeType.Framework,
                null, null),
            DeliveryModel = draftApprenticeship.DeliveryModel,
            Cost = (int?) draftApprenticeship.Cost,
            StartDate = draftApprenticeship.StartDate,
            EndDate = draftApprenticeship.EndDate,
            DateOfBirth = draftApprenticeship.DateOfBirth,
            Reference = draftApprenticeship.ProviderRef,
            ReservationId = draftApprenticeship.ReservationId,
            IsOnFlexiPaymentPilot = draftApprenticeship.IsOnFlexiPaymentPilot
        };
    }

    public static void AssertSameProperties(DraftApprenticeship expected, DraftApprenticeship actual)
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

    private static void AssertSameProperty<P>(DraftApprenticeship expected, DraftApprenticeship actual, Expression<Func<DraftApprenticeship, P>> action)
    {
        var expression = (MemberExpression) action.Body;
        var name = expression.Member.Name;

        var propertyInfo = expression.Member as PropertyInfo;

        var expectedValue = propertyInfo.GetValue(expected);
        var actualValue = propertyInfo.GetValue(actual);

        Assert.That(actualValue, Is.EqualTo(expectedValue), $"{name} is not equal");
    }

    private static string SafeUpdate(string src)
    {
        return (src ?? "") + "X";
    }

    private static DateTime SafeUpdate(DateTime? src)
    {
        return (src ?? DateTime.Today).AddDays(1);
    }
}