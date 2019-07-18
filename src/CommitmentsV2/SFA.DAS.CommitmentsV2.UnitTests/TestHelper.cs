using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.UnitTests
{
    public class TestHelper
    {
        public static T Clone<T>(T source)
        {
            var settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            var serialized = JsonConvert.SerializeObject(source, settings);
            return JsonConvert.DeserializeObject<T>(serialized, settings);
        }
    }
}