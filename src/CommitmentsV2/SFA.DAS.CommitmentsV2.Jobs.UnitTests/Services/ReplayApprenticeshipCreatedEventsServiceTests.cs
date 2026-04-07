using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.Services;

public class ReplayApprenticeshipCreatedEventsServiceTests
{
    [Test]
    public void Then_Csv_Content_Is_Parsed_To_Cohort_Ids()
    {
        var result = ReplayApprenticeshipCreatedEventsService.ParseCohortIds("123,456\n789;101112");
        result.Should().BeEquivalentTo([123L, 456L, 789L, 101112L]);
    }

    [Test]
    public void Then_Quoted_Csv_Values_Are_Parsed_To_Cohort_Ids()
    {
        var result = ReplayApprenticeshipCreatedEventsService.ParseCohortIds("\"114742\",'223344'");
        result.Should().BeEquivalentTo([114742L, 223344L]);
    }
}
