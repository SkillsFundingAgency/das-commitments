using System.Collections.Generic;
using System.Reflection;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SFA.DAS.Commitments.Application.UnitTests
{
    public static class TestHelper
    {
        public static T Clone<T>(T source)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver()
            };

            var serialized = JsonConvert.SerializeObject(source, settings);
            return JsonConvert.DeserializeObject<T>(serialized, settings);
        }

        public static bool EnumerablesAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true })
                .Compare(expected, actual).AreEqual;
        }
    }

    public class JsonIgnoreAttributeIgnorerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.Ignored = false;
            return property;
        }
    }
}
