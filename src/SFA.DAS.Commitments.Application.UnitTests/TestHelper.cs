using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Application.UnitTests
{
    public static class TestHelper
    {
        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
