using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.UnitTests
{
    public class TestHelper
    {
        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
