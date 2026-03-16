using FluentAssertions;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers;
public class SyncLearningDataBatchCommandHandlerTests
{
    [Test]
    public void Should_Map_All_Fields_Correctly()
    {
        // Arrange
        var fixture = new Fixture();
        var account = new Account(1, "", "", "", DateTime.UtcNow);
        var accountLegalEntity = new AccountLegalEntity(account, 1, 2, "", "", "", OrganisationType.CompaniesHouse,
            "", DateTime.UtcNow);

        var apprenticeship = new Apprenticeship
        {
            Id = 123,
            Uln = fixture.Create<string>(),
            CourseCode = fixture.Create<string>(),
            StandardUId = fixture.Create<string>(),
            TrainingCourseOption = fixture.Create<string>(),
            TrainingCourseVersion = fixture.Create<string>(),
            StartDate = fixture.Create<DateTime>(),
            EndDate = fixture.Create<DateTime>(),
            DateOfBirth = fixture.Create<DateTime>(),
            ActualStartDate = new DateTime(2024, 1, 10),
            FirstName = fixture.Create<string>(),
            LastName = fixture.Create<string>(),
            ContinuationOfId = 456,
            LearnerDataId = 789,
            ProgrammeType = ProgrammeType.Standard,
            DeliveryModel = DeliveryModel.Regular,
            PriceHistory = [ new PriceHistory {
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 12, 31),
                Cost = 10000,
                AssessmentPrice = 2000,
                TrainingPrice = 8000 }
            ],
            Cohort = new Cohort
            {
                EmployerAndProviderApprovedOn = new DateTime(2023, 12, 1),
                EmployerAccountId = 111,
                ProviderId = 222,
                TransferSenderId = 333,
                AccountLegalEntity = accountLegalEntity
            }
        };

        // Build expected result (except CreatedOn)
        var expected = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipId = 123,
            AgreedOn = new DateTime(2023, 12, 1),
            AccountId = 111,
            AccountLegalEntityPublicHashedId = "",
            AccountLegalEntityId = 444,
            LegalEntityName = "",
            ProviderId = 222,
            TransferSenderId = 333,
            ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy,
            Uln = apprenticeship.Uln,
            DeliveryModel = DeliveryModel.Regular,
            TrainingType = ProgrammeType.Standard,
            TrainingCode = apprenticeship.CourseCode,
            StandardUId = apprenticeship.StandardUId,
            TrainingCourseOption = apprenticeship.TrainingCourseOption,
            TrainingCourseVersion = apprenticeship.TrainingCourseVersion,
            StartDate = apprenticeship.StartDate.Value,
            EndDate = apprenticeship.EndDate.Value,
            PriceEpisodes =
            [
                new PriceEpisode
            {
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 12, 31),
                Cost = 10000,
                EndPointAssessmentPrice = 2000,
                TrainingPrice = 8000
            }
            ],
            ContinuationOfId = 456,
            DateOfBirth = apprenticeship.DateOfBirth.Value,
            ActualStartDate = new DateTime(2024, 1, 10),
            FirstName = apprenticeship.FirstName,
            LastName = apprenticeship.LastName,
            ApprenticeshipHashedId = "",
            LearnerDataId = 789,
            LearningType = Common.Domain.Types.LearningType.Apprenticeship
        };

        // Act
        var result = SyncLearningDataBatchCommandHandler.CreateEventFromApprenticeship(apprenticeship);

        // Assert
        result.Should().BeEquivalentTo(expected, options =>
            options.Excluding(e => e.CreatedOn)
                .Excluding(e => e.AccountLegalEntityPublicHashedId)
                .Excluding(e => e.AccountLegalEntityId)
                .Excluding(e => e.LegalEntityName));
    }
}
