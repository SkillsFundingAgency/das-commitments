using AutoFixture.NUnit3;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCalculatedTrainingProgrammeVersion
{
    public class WhenGettingATrainingProgrammeVersion
    {
        [Test, MoqAutoData]
        public async Task And_DataIsRetrievedFromLookupService_Then_ReturnData(
            GetCalculatedTrainingProgrammeVersionQuery query,
            [Frozen]Mock<ITrainingProgrammeLookup> service,
            TrainingProgramme result,
            GetCalculatedTrainingProgrammeVersionQueryHandler handler)
        {
            service.Setup(s => s.GetCalculatedTrainingProgrammeVersion(query.CourseCode.ToString(), query.StartDate))
                .ReturnsAsync(result);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgramme.Should().BeEquivalentTo(result);
        }

        [Test, MoqAutoData]
        public async Task And_TrainingProgrammeReturnsNull_Then_ReturnEmptyQueryResult(
            GetCalculatedTrainingProgrammeVersionQuery query,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetCalculatedTrainingProgrammeVersionQueryHandler handler)
        {
            service.Setup(s => s.GetCalculatedTrainingProgrammeVersion(query.CourseCode.ToString(), query.StartDate))
                .ReturnsAsync((TrainingProgramme)null);

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Should().BeOfType<GetCalculatedTrainingProgrammeVersionQueryResult>();
            actual.TrainingProgramme.Should().BeNull();
        }
    }
}
