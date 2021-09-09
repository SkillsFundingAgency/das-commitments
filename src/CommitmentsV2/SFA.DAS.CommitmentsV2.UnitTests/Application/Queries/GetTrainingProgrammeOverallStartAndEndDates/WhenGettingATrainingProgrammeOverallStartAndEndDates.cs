using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCalculatedTrainingProgrammeVersion
{
    public class WhenGettingATrainingProgrammeOverallStartAndEndDates
    {
        [Test, MoqAutoData]
        public async Task And_DataIsRetrievedFromLookupService_Then_ReturnData(
            GetTrainingProgrammeOverallStartAndEndDatesQuery query,
            [Frozen]Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeOverallStartAndEndDatesQueryResult result,
            GetTrainingProgrammeOverallStartAndEndDatesQueryHandler handler)
        {
            service.Setup(s => s.GetTrainingProgrammeOverallStartAndEndDates(query.CourseCode.ToString()))
                .ReturnsAsync((result.TrainingProgrammeEffectiveFrom, result.TrainingProgrammeEffectiveTo));

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Should().BeEquivalentTo(result);
        }

        [Test, MoqAutoData]
        public async Task And_TrainingProgrammeVersionsDontExistReturnsNull_Then_ReturnEmptyQueryResult(
            GetTrainingProgrammeOverallStartAndEndDatesQuery query,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeOverallStartAndEndDatesQueryHandler handler)
        {
            service.Setup(s => s.GetTrainingProgrammeOverallStartAndEndDates(query.CourseCode.ToString()))
                .ReturnsAsync((null,null));

            var actual = await handler.Handle(query, CancellationToken.None);

            actual.Should().BeOfType<GetTrainingProgrammeOverallStartAndEndDatesQueryResult>();
            actual.TrainingProgrammeEffectiveFrom.Should().BeNull();
            actual.TrainingProgrammeEffectiveTo.Should().BeNull();
        }
    }
}
