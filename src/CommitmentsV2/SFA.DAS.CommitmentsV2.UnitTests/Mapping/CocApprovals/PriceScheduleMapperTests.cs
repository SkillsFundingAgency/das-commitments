using Newtonsoft.Json.Linq;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CocApprovals;

[TestFixture]
public class PriceScheduleMapperTests
{
    private PriceScheduleMapper _mapper;

    [SetUp]
    public void SetUp()
    {
        _mapper = new PriceScheduleMapper();
    }

    [Test]
    public void Map_CostOnlyHistory_WritesBothAmountsNull()
    {
        var json = _mapper.Map(new[]
        {
            History(new DateTime(2024, 9, 1), 2000m, null, null),
            History(new DateTime(2024, 8, 1), 1500m, 1500m, null)
        });

        var episodes = JArray.Parse(json);
        episodes.Should().HaveCount(2);
        episodes[0]["effectiveTo"].Should().BeNull();
        Episode(episodes[0], "2024-08-01", 1500m, null, null);
        Episode(episodes[1], "2024-09-01", 2000m, null, null);
    }

    [Test]
    public void Map_HistoryWithBothPrices_CopiesAmountsAndKeepsCost()
    {
        var json = _mapper.Map(new[]
        {
            History(new DateTime(2024, 8, 1), 1600m, 1000m, 500m)
        });

        var episodes = JArray.Parse(json);
        Episode(episodes.Should().ContainSingle().Subject, "2024-08-01", 1600m, 1000m, 500m);
    }

    [Test]
    public void Map_SubmittedEpisodes_SetsCostToTheSum()
    {
        var json = _mapper.Map(new[]
        {
            Record(new DateTime(2024, 8, 1), 1000m, 500m),
            Record(new DateTime(2024, 9, 1), 1200m, 800m)
        });

        var episodes = JArray.Parse(json);
        Episode(episodes[0], "2024-08-01", 1500m, 1000m, 500m);
        Episode(episodes[1], "2024-09-01", 2000m, 1200m, 800m);
    }

    [Test]
    public void AreEqual_MatchingSchedules_IgnoresOrder()
    {
        var history = _mapper.Map(new[]
        {
            History(new DateTime(2024, 9, 1), 2000m, 1200m, 800m),
            History(new DateTime(2024, 8, 1), 1500m, 1000m, 500m)
        });
        var submitted = _mapper.Map(new[]
        {
            Record(new DateTime(2024, 9, 1), 1200m, 800m),
            Record(new DateTime(2024, 8, 1), 1000m, 500m)
        });

        _mapper.AreEqual(history, submitted).Should().BeTrue();
    }

    [Test]
    public void AreEqual_CostOnlySchedules_MatchOnCost()
    {
        var first = _mapper.Map(new[]
        {
            History(new DateTime(2024, 9, 1), 2000m, null, null),
            History(new DateTime(2024, 8, 1), 1500m, null, 400m)
        });
        var second = _mapper.Map(new[]
        {
            History(new DateTime(2024, 8, 1), 1500m, 1500m, null),
            History(new DateTime(2024, 9, 1), 2000m, null, null)
        });

        _mapper.AreEqual(first, second).Should().BeTrue();
    }

    [Test]
    public void AreEqual_ReversalMatchesManuallyAddedCostOnlyHistoryOnCost()
    {
        var history = _mapper.Map(new[]
        {
            History(new DateTime(2024, 8, 1), 1500m, null, null)
        });
        var change = _mapper.Map(new[]
        {
            Record(new DateTime(2024, 8, 1), 2000m, 800m)
        });
        var reversal = _mapper.Map(new[]
        {
            Record(new DateTime(2024, 8, 1), 1000m, 500m)
        });

        _mapper.AreEqual(history, change).Should().BeFalse();
        _mapper.AreEqual(history, reversal).Should().BeTrue();
    }

    [Test]
    public void AreEqual_SplitHistory_RequiresTheSameAmounts()
    {
        var history = _mapper.Map(new[]
        {
            History(new DateTime(2024, 8, 1), 1500m, 1000m, 500m)
        });
        var differentSplit = _mapper.Map(new[]
        {
            Record(new DateTime(2024, 8, 1), 1200m, 300m)
        });

        _mapper.AreEqual(history, differentSplit).Should().BeFalse();
    }

    [TestCase(null, 500)]
    [TestCase(1000, null)]
    public void Map_SubmittedEpisodeMissingAmount_Throws(int? trainingPrice, int? assessmentPrice)
    {
        var act = () => _mapper.Map(new[]
        {
            new PriceRecord
            {
                TrainingPrice = trainingPrice,
                AssessmentPrice = assessmentPrice,
                EffectiveFrom = new DateTime(2024, 8, 1)
            }
        });

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Map_OneHundredHistoryEpisodes_MapsAllOfThem()
    {
        var history = Enumerable.Range(0, Constants.MaximumPriceEpisodes)
            .Select(index => History(new DateTime(2020, 1, 1).AddDays(index), index + 1, null, null))
            .Reverse();

        var episodes = JArray.Parse(_mapper.Map(history));

        episodes.Should().HaveCount(Constants.MaximumPriceEpisodes);
        episodes.First()["effectiveFrom"].Value<string>().Should().Be("2020-01-01");
        episodes.Last()["effectiveFrom"].Value<string>().Should().Be(
            new DateTime(2020, 1, 1).AddDays(Constants.MaximumPriceEpisodes - 1).ToString("yyyy-MM-dd"));
    }

    [Test]
    public void Map_HistoryLongerThanTheRequestLimit_IsNotCapped()
    {
        var history = Enumerable.Range(0, Constants.MaximumPriceEpisodes + 1)
            .Select(index => History(new DateTime(2020, 1, 1).AddDays(index), 10m, 6m, 4m));

        var episodes = JArray.Parse(_mapper.Map(history));

        episodes.Should().HaveCount(Constants.MaximumPriceEpisodes + 1);
    }

    private static void Episode(JToken episode, string effectiveFrom, decimal cost, decimal? trainingPrice, decimal? assessmentPrice)
    {
        episode["effectiveFrom"].Value<string>().Should().Be(effectiveFrom);
        episode["cost"].Value<decimal>().Should().Be(cost);
        Amount(episode["trainingPrice"], trainingPrice);
        Amount(episode["assessmentPrice"], assessmentPrice);
    }

    private static void Amount(JToken token, decimal? expected)
    {
        if (expected == null)
        {
            token.Type.Should().Be(JTokenType.Null);
            return;
        }

        token.Value<decimal>().Should().Be(expected.Value);
    }

    private static PriceHistory History(DateTime fromDate, decimal cost, decimal? trainingPrice, decimal? assessmentPrice)
    {
        return new PriceHistory
        {
            FromDate = fromDate,
            Cost = cost,
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice
        };
    }

    private static PriceRecord Record(DateTime effectiveFrom, decimal trainingPrice, decimal assessmentPrice)
    {
        return new PriceRecord
        {
            EffectiveFrom = effectiveFrom,
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice
        };
    }
}
