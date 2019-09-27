using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source);
        }
    }
}