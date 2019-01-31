using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Api.UnitTests
{
    public static class TestHelper
    {
        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static bool EnumerablesAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true })
                .Compare(expected, actual).AreEqual;
        }
    }
}
