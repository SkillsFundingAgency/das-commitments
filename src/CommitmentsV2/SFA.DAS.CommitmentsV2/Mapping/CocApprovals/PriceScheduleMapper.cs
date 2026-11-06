using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Mapping.CocApprovals;

public class PriceScheduleMapper
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Include,
        Formatting = Formatting.None
    };

    public string Map(IEnumerable<PriceHistory> priceHistory)
    {
        ArgumentNullException.ThrowIfNull(priceHistory);

        var episodes = priceHistory
            .OrderBy(history => history.FromDate)
            .Select(ToEpisode)
            .ToList();

        return Serialize(episodes);
    }

    public string Map(IEnumerable<PriceRecord> priceRecords)
    {
        return Serialize(ToSubmittedEpisodes(priceRecords));
    }

    public bool LatestEpisodesMatch(IEnumerable<PriceHistory> priceHistory, IEnumerable<PriceRecord> priceRecords)
    {
        ArgumentNullException.ThrowIfNull(priceHistory);
        ArgumentNullException.ThrowIfNull(priceRecords);

        var current = Latest(priceHistory.Select(ToEpisode));
        var submitted = Latest(ToSubmittedEpisodes(priceRecords));
        if (current == null || submitted == null)
        {
            return false;
        }

        return PriceScheduleEpisodeComparer.Instance.Equals(current, submitted);
    }

    public bool AreEqual(string left, string right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftEpisodes = Ordered(Deserialize(left));
        var rightEpisodes = Ordered(Deserialize(right));

        return leftEpisodes.SequenceEqual(rightEpisodes, PriceScheduleEpisodeComparer.Instance);
    }

    public static decimal? GetLatestCost(string scheduleJson)
    {
        if (string.IsNullOrWhiteSpace(scheduleJson))
        {
            return null;
        }

        try
        {
            return Latest(Deserialize(scheduleJson))?.Cost;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static PriceScheduleEpisode ToEpisode(PriceHistory history)
    {
        var hasSplit = history.TrainingPrice.HasValue && history.AssessmentPrice.HasValue;

        return new PriceScheduleEpisode
        {
            EffectiveFrom = FormatDate(history.FromDate),
            Cost = history.Cost,
            TrainingPrice = hasSplit ? history.TrainingPrice : null,
            AssessmentPrice = hasSplit ? history.AssessmentPrice : null
        };
    }

    private static List<PriceScheduleEpisode> ToSubmittedEpisodes(IEnumerable<PriceRecord> priceRecords)
    {
        ArgumentNullException.ThrowIfNull(priceRecords);

        var episodes = new List<PriceScheduleEpisode>();
        foreach (var priceRecord in priceRecords)
        {
            if (priceRecord?.TrainingPrice == null || priceRecord?.AssessmentPrice == null)
            {
                throw new ArgumentException("A price episode must include both trainingPrice and assessmentPrice.");
            }

            if (priceRecord.EffectiveFrom == null)
            {
                throw new ArgumentException("A price episode must include an effectiveFrom date.");
            }

            episodes.Add(new PriceScheduleEpisode
            {
                EffectiveFrom = FormatDate(priceRecord.EffectiveFrom.Value),
                Cost = priceRecord.TrainingPrice.Value + priceRecord.AssessmentPrice.Value,
                TrainingPrice = priceRecord.TrainingPrice,
                AssessmentPrice = priceRecord.AssessmentPrice
            });
        }

        return episodes;
    }

    private static PriceScheduleEpisode Latest(IEnumerable<PriceScheduleEpisode> episodes)
    {
        return episodes
            .OrderBy(episode => episode.EffectiveFrom, StringComparer.Ordinal)
            .LastOrDefault();
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string Serialize(IEnumerable<PriceScheduleEpisode> episodes)
    {
        return JsonConvert.SerializeObject(episodes, Settings);
    }

    private static List<PriceScheduleEpisode> Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<List<PriceScheduleEpisode>>(json, Settings) ?? [];
    }

    private static List<PriceScheduleEpisode> Ordered(IEnumerable<PriceScheduleEpisode> episodes)
    {
        return episodes
            .OrderBy(episode => episode.EffectiveFrom, StringComparer.Ordinal)
            .ThenBy(episode => episode.Cost)
            .ThenBy(episode => episode.TrainingPrice.HasValue)
            .ThenBy(episode => episode.TrainingPrice ?? 0)
            .ThenBy(episode => episode.AssessmentPrice.HasValue)
            .ThenBy(episode => episode.AssessmentPrice ?? 0)
            .ToList();
    }

    private sealed class PriceScheduleEpisode
    {
        public string EffectiveFrom { get; set; }
        public decimal Cost { get; set; }
        public decimal? TrainingPrice { get; set; }
        public decimal? AssessmentPrice { get; set; }

        public bool IsCostOnly()
        {
            return TrainingPrice == null && AssessmentPrice == null;
        }
    }

    private sealed class PriceScheduleEpisodeComparer : IEqualityComparer<PriceScheduleEpisode>
    {
        public static readonly PriceScheduleEpisodeComparer Instance = new();

        public bool Equals(PriceScheduleEpisode x, PriceScheduleEpisode y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.EffectiveFrom != y.EffectiveFrom || x.Cost != y.Cost)
            {
                return false;
            }

            if (x.IsCostOnly() || y.IsCostOnly())
            {
                return true;
            }

            return x.TrainingPrice == y.TrainingPrice
                && x.AssessmentPrice == y.AssessmentPrice;
        }

        public int GetHashCode(PriceScheduleEpisode obj)
        {
            return HashCode.Combine(obj.EffectiveFrom, obj.Cost);
        }
    }
}